using System.Text.Json.Serialization;
using SmartX.Gateway.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GatewayStore>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartXClient", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("SmartXClient");
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/healthz", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/summary", (GatewayStore store) =>
{
    lock (store.SyncRoot)
    {
        var acceptedRate = store.Telemetry.Count == 0
            ? 100
            : Math.Round(store.Telemetry.Count(item => item.Quality != ReadingQuality.Rejected) * 100d / store.Telemetry.Count, 1);
        var summary = new GatewaySummary(
            store.Sensors.Count,
            store.Sensors.Count(item => item.Status == SensorStatus.Online),
            store.Sensors.Sum(item => item.ReadingsToday),
            acceptedRate,
            store.Activity.Count(item => item.Tone == ActivityTone.Warning),
            store.Telemetry.FirstOrDefault()?.RecordedAt ?? DateTimeOffset.UtcNow,
            store.HistoricalTelemetryBatch.GetLength(0),
            ValidateDeploymentTree(store.DeploymentRoot).NodeCount,
            store.Attachments.Count);
        return Results.Ok(summary);
    }
});

app.MapGet("/api/topology/validation", (GatewayStore store) =>
{
    var result = ValidateDeploymentTree(store.DeploymentRoot);
    return Results.Ok(new { result.Valid, result.NodeCount, message = "Nested deployment topology passed recursive validation." });
});

app.MapGet("/api/telemetry/history-grid", (GatewayStore store) =>
{
    var rows = Enumerable.Range(0, store.HistoricalTelemetryBatch.GetLength(0))
        .Select(row => Enumerable.Range(0, store.HistoricalTelemetryBatch.GetLength(1))
            .Select(column => store.HistoricalTelemetryBatch[row, column])
            .ToArray())
        .ToArray();
    return Results.Ok(rows);
});

app.MapGet("/api/sensors", (GatewayStore store) =>
{
    lock (store.SyncRoot)
    {
        return Results.Ok(store.Sensors);
    }
});

app.MapPost("/api/sensors", (SensorInput input, GatewayStore store) =>
{
    if (string.IsNullOrWhiteSpace(input.MacAddress) || input.MacAddress.Length < 12 ||
        string.IsNullOrWhiteSpace(input.Location))
    {
        return Results.BadRequest(new { error = "MAC address and location are required." });
    }

    lock (store.SyncRoot)
    {
        var sensor = new Sensor(store.NextSensorId(), input.MacAddress, input.Location, input.Category,
            SensorStatus.Online, DateTimeOffset.UtcNow, 0);
        store.Sensors.Insert(0, sensor);
        store.Activity.Insert(0, new ActivityEvent(store.NextActivityId(), "Sensor registered",
            $"{sensor.Location} is now available to the gateway.", ActivityTone.Success, DateTimeOffset.UtcNow));
        return Results.Created($"/api/sensors/{sensor.Id}", sensor);
    }
});

app.MapGet("/api/telemetry", (int? limit, GatewayStore store) =>
{
    var take = Math.Clamp(limit ?? 20, 1, 100);
    lock (store.SyncRoot)
    {
        return Results.Ok(store.Telemetry.Take(take));
    }
});

app.MapPost("/api/validation", (TelemetryInput input, GatewayStore store) =>
    Results.Ok(Validate(ToPacket(input), store)));

app.MapPost("/api/telemetry", (TelemetryInput input, GatewayStore store) =>
{
    var packet = ToPacket(input);
    var validation = Validate(packet, store);
    if (!validation.Valid)
    {
        return Results.Json(new IngestionResult(false, null, validation), statusCode: StatusCodes.Status202Accepted);
    }

    lock (store.SyncRoot)
    {
        var sensor = store.Sensors.First(item => item.Id == packet.SensorId);
        var reading = new TelemetryReading(store.NextReadingId(), sensor.Id, sensor.Location,
            packet.Metric, packet.Value, packet.Unit, packet.RecordedAt ?? DateTimeOffset.UtcNow,
            validation.Warnings.Count > 0 ? ReadingQuality.Warning : ReadingQuality.Valid);
        store.Telemetry.Insert(0, reading);
        var sensorIndex = store.Sensors.FindIndex(item => item.Id == sensor.Id);
        store.Sensors[sensorIndex] = sensor with
        {
            LastSeen = reading.RecordedAt,
            ReadingsToday = sensor.ReadingsToday + 1,
            Status = sensor.Status == SensorStatus.Offline ? SensorStatus.Online : sensor.Status
        };
        store.Activity.Insert(0, new ActivityEvent(store.NextActivityId(), "Telemetry accepted",
            $"{reading.Metric} from {reading.SensorLabel} passed gateway validation.",
            validation.Warnings.Count > 0 ? ActivityTone.Warning : ActivityTone.Success,
            DateTimeOffset.UtcNow));
        return Results.Json(new IngestionResult(true, reading, validation), statusCode: StatusCodes.Status202Accepted);
    }
});

app.MapGet("/api/activity", (GatewayStore store) =>
{
    lock (store.SyncRoot)
    {
        return Results.Ok(store.Activity.Take(10));
    }
});

app.MapGet("/api/engagement", (GatewayStore store) =>
{
    lock (store.SyncRoot)
    {
        return Results.Ok(store.Engagement);
    }
});

app.MapPut("/api/engagement", (EngagementStrategyInput input, GatewayStore store) =>
{
    lock (store.SyncRoot)
    {
        store.Engagement = store.Engagement with
        {
            Name = input.Name,
            Description = input.Description,
            Enabled = input.Enabled,
            Trigger = input.Trigger,
            Delivery = input.Delivery,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        return Results.Ok(store.Engagement);
    }
});

app.MapGet("/api/uploads", (GatewayStore store) =>
{
    lock (store.SyncRoot)
    {
        return Results.Ok(store.Attachments);
    }
});

app.MapPost("/api/uploads", async (HttpRequest request, GatewayStore store) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest(new { error = "Use multipart/form-data with a file field." });

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0)
        return Results.BadRequest(new { error = "Choose a non-empty configuration, photo, or log file." });
    if (file.Length > 10 * 1024 * 1024)
        return Results.BadRequest(new { error = "Files must be 10 MB or smaller." });

    var originalName = Path.GetFileName(file.FileName);
    var extension = Path.GetExtension(originalName).ToLowerInvariant();
    var attachmentType = extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "Deployment photo",
        ".log" or ".txt" or ".csv" => "Hardware log",
        _ => "Device configuration"
    };
    var directory = Path.Combine(AppContext.BaseDirectory, "uploads");
    Directory.CreateDirectory(directory);
    var storedName = $"{Guid.NewGuid():N}-{originalName}";
    await using (var stream = File.Create(Path.Combine(directory, storedName)))
    {
        await file.CopyToAsync(stream);
    }

    var attachment = new MediaAttachment(store.NextAttachmentId(), originalName,
        file.ContentType, file.Length, attachmentType, DateTimeOffset.UtcNow);
    lock (store.SyncRoot)
    {
        store.Attachments.Insert(0, attachment);
        store.Activity.Insert(0, new ActivityEvent(store.NextActivityId(), "Media attached",
            $"{attachmentType} {originalName} was attached to the gateway record.",
            ActivityTone.Info, DateTimeOffset.UtcNow));
    }
    return Results.Created("/api/uploads", attachment);
});

app.Run();

static TelemetryPacket<double> ToPacket(TelemetryInput input) =>
    new(input.SensorId, input.Metric, input.Value, input.Unit, input.RecordedAt);

static ValidationResult Validate(TelemetryPacket<double> packet, GatewayStore store)
{
    var errors = new List<string>();
    var warnings = new List<string>();
    Sensor? sensor;
    lock (store.SyncRoot)
    {
        sensor = store.Sensors.FirstOrDefault(item => item.Id == packet.SensorId);
    }

    if (sensor is null) errors.Add("The sensor ID is not registered.");
    if (!double.IsFinite(packet.Value)) errors.Add("The reading value must be numeric.");
    if (string.IsNullOrWhiteSpace(packet.Metric) || packet.Metric.Length < 2) errors.Add("The metric name is too short.");
    if (string.IsNullOrWhiteSpace(packet.Unit)) errors.Add("The unit is required.");
    var currentValue = new TelemetryValue(packet.Value);
    if (packet.Metric.Contains("temperature", StringComparison.OrdinalIgnoreCase) &&
        (currentValue < new TelemetryValue(-40) || currentValue > new TelemetryValue(85)))
        warnings.Add("Temperature is outside the expected operating band of -40°C to 85°C.");
    if (packet.Metric.Contains("power", StringComparison.OrdinalIgnoreCase) && currentValue > new TelemetryValue(4.5))
        warnings.Add("Power draw is above the configured warning threshold.");
    if (packet.RecordedAt.HasValue && packet.RecordedAt.Value > DateTimeOffset.UtcNow.AddMinutes(5))
        warnings.Add("The timestamp is in the future and should be reviewed.");

    return new ValidationResult(errors.Count == 0, errors, warnings);
}

static (bool Valid, int NodeCount) ValidateDeploymentTree(DeploymentNode node)
{
    // Base case: a leaf sensor is a valid single-node deployment.
    if (node.Children.Count == 0)
        return (!string.IsNullOrWhiteSpace(node.Name), 1);

    var valid = !string.IsNullOrWhiteSpace(node.Name);
    var nodeCount = 1;
    foreach (var child in node.Children)
    {
        var childResult = ValidateDeploymentTree(child);
        valid &= childResult.Valid;
        nodeCount += childResult.NodeCount;
    }
    return (valid, nodeCount);
}