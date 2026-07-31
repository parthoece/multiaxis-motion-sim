namespace MotionControl.Application;

public sealed record OperationalWarning(
    string EventName,
    string Message,
    DateTimeOffset Timestamp);
