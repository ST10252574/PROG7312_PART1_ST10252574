namespace SmartX.Gateway.Api;

public sealed class GatewayStore
{
    private readonly object sync = new();
    private int sensorSequence = 4;
    private int readingSequence = 4;
    private int activitySequence = 3;
    private int attachmentSequence;

    public List<Sensor> Sensors { get; } =
    [
        new("sensor-001", "A4:C1:38:7F:20:11", "Cape Town / Workshop", SensorCategory.Environmental, SensorStatus.Online, DateTimeOffset.UtcNow, 148),
        new("sensor-002", "B8:27:EB:44:19:02", "Johannesburg / Rooftop", SensorCategory.Power, SensorStatus.Warning, DateTimeOffset.UtcNow.AddMinutes(-7), 96),
        new("sensor-003", "D0:39:72:AA:03:8C", "Durban / Pump Room", SensorCategory.Actuator, SensorStatus.Online, DateTimeOffset.UtcNow.AddMinutes(-2), 83),
        new("sensor-004", "58:BF:25:9E:77:41", "Pretoria / Loading Bay", SensorCategory.Environmental, SensorStatus.Offline, DateTimeOffset.UtcNow.AddMinutes(-48), 12)
    ];

    public List<TelemetryReading> Telemetry { get; } =
    [
        new("reading-001", "sensor-001", "Cape Town / Workshop", "Temperature", 23.4, "°C", DateTimeOffset.UtcNow.AddMinutes(-2), ReadingQuality.Valid),
        new("reading-002", "sensor-002", "Johannesburg / Rooftop", "Power draw", 4.82, "kW", DateTimeOffset.UtcNow.AddMinutes(-4), ReadingQuality.Warning),
        new("reading-003", "sensor-003", "Durban / Pump Room", "Flow rate", 18.7, "L/min", DateTimeOffset.UtcNow.AddMinutes(-5), ReadingQuality.Valid),
        new("reading-004", "sensor-001", "Cape Town / Workshop", "Humidity", 51, "%", DateTimeOffset.UtcNow.AddMinutes(-8), ReadingQuality.Valid)
    ];

    public List<ActivityEvent> Activity { get; } =
    [
        new("activity-001", "Mesh heartbeat restored", "Durban / Pump Room is responding within the expected interval.", ActivityTone.Success, DateTimeOffset.UtcNow.AddMinutes(-3)),
        new("activity-002", "Power threshold crossed", "Johannesburg / Rooftop reported 4.82 kW against a 4.5 kW warning threshold.", ActivityTone.Warning, DateTimeOffset.UtcNow.AddMinutes(-7)),
        new("activity-003", "Telemetry batch accepted", "12 readings passed schema and range validation.", ActivityTone.Info, DateTimeOffset.UtcNow.AddMinutes(-12))
    ];

    // A raw multi-dimensional batch is retained before readings are normalised
    // into the optimised List<T> collections above.
    public double[,] HistoricalTelemetryBatch { get; } =
    {
        { 23.4, 23.8, 24.1 },
        { 4.21, 4.35, 4.82 },
        { 18.7, 19.1, 18.4 },
        { 51.0, 50.4, 51.2 }
    };

    // Nested deployment configuration used by the recursive topology validator.
    public DeploymentNode DeploymentRoot { get; } =
        new("Smart-X Gateway", "gateway",
        [
            new("Cape Town / Workshop", "site",
            [
                new("sensor-001", "sensor", [])
            ]),
            new("Johannesburg / Rooftop", "site",
            [
                new("sensor-002", "sensor", [])
            ]),
            new("Durban / Pump Room", "site",
            [
                new("sensor-003", "sensor", [])
            ])
        ]);

    public List<MediaAttachment> Attachments { get; } = [];

    public EngagementStrategy Engagement { get; set; } =
        new("strategy-001", "Proactive operator alerts",
            "Surface threshold crossings, device disconnects, and validation warnings in the dashboard so operators can respond before data quality degrades.",
            true, EngagementTrigger.Threshold, EngagementDelivery.Dashboard, DateTimeOffset.UtcNow);

    public string NextSensorId() => $"sensor-{Interlocked.Increment(ref sensorSequence):000}";
    public string NextReadingId() => $"reading-{Interlocked.Increment(ref readingSequence):000}";
    public string NextActivityId() => $"activity-{Interlocked.Increment(ref activitySequence):000}";
    public string NextAttachmentId() => $"attachment-{Interlocked.Increment(ref attachmentSequence):000}";

    public object SyncRoot => sync;
}