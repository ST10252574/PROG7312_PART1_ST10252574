namespace SmartX.Gateway.Api;

public enum SensorCategory
{
    Environmental,
    Power,
    Actuator
}

public enum SensorStatus
{
    Online,
    Warning,
    Offline
}

public enum ReadingQuality
{
    Valid,
    Warning,
    Rejected
}

public enum ActivityTone
{
    Success,
    Warning,
    Info
}

public enum EngagementTrigger
{
    Threshold,
    Disconnect,
    Anomaly
}

public enum EngagementDelivery
{
    Dashboard,
    Email,
    Webhook
}

public sealed record Sensor(
    string Id,
    string MacAddress,
    string Location,
    SensorCategory Category,
    SensorStatus Status,
    DateTimeOffset LastSeen,
    int ReadingsToday);

public sealed record SensorInput(
    string MacAddress,
    string Location,
    SensorCategory Category);

public sealed record TelemetryInput(
    string SensorId,
    string Metric,
    double Value,
    string Unit,
    DateTimeOffset? RecordedAt);

// Generic wrapper used by the gateway to carry different telemetry value types
// through the same ingestion pipeline without using unstructured strings.
public sealed record TelemetryPacket<T>(
    string SensorId,
    string Metric,
    T Value,
    string Unit,
    DateTimeOffset? RecordedAt);

// Strongly typed value object with domain-specific arithmetic and comparisons.
public readonly record struct TelemetryValue(double Amount) : IComparable<TelemetryValue>
{
    public int CompareTo(TelemetryValue other) => Amount.CompareTo(other.Amount);
    public static TelemetryValue operator +(TelemetryValue left, TelemetryValue right) => new(left.Amount + right.Amount);
    public static TelemetryValue operator -(TelemetryValue left, TelemetryValue right) => new(left.Amount - right.Amount);
    public static bool operator >(TelemetryValue left, TelemetryValue right) => left.Amount > right.Amount;
    public static bool operator <(TelemetryValue left, TelemetryValue right) => left.Amount < right.Amount;
    public static bool operator >=(TelemetryValue left, TelemetryValue right) => left.Amount >= right.Amount;
    public static bool operator <=(TelemetryValue left, TelemetryValue right) => left.Amount <= right.Amount;
}

public sealed record TelemetryReading(
    string Id,
    string SensorId,
    string SensorLabel,
    string Metric,
    double Value,
    string Unit,
    DateTimeOffset RecordedAt,
    ReadingQuality Quality);

public sealed record ValidationResult(
    bool Valid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

public sealed record IngestionResult(
    bool Accepted,
    TelemetryReading? Reading,
    ValidationResult Validation);

public sealed record ActivityEvent(
    string Id,
    string Title,
    string Detail,
    ActivityTone Tone,
    DateTimeOffset OccurredAt);

public sealed record GatewaySummary(
    int RegisteredSensors,
    int OnlineSensors,
    int ReadingsToday,
    double AcceptedRate,
    int Alerts,
    DateTimeOffset LastIngestedAt,
    int HistoricalBatchRows,
    int DeploymentNodes,
    int Attachments);

public sealed record DeploymentNode(
    string Name,
    string NodeType,
    IReadOnlyList<DeploymentNode> Children);

public sealed record MediaAttachment(
    string Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string AttachmentType,
    DateTimeOffset UploadedAt);

public sealed record EngagementStrategy(
    string Id,
    string Name,
    string Description,
    bool Enabled,
    EngagementTrigger Trigger,
    EngagementDelivery Delivery,
    DateTimeOffset UpdatedAt);

public sealed record EngagementStrategyInput(
    string Name,
    string Description,
    bool Enabled,
    EngagementTrigger Trigger,
    EngagementDelivery Delivery);