namespace MotionControl.Domain;

public sealed record AlarmRecord(
    Guid AlarmId,
    FaultCode Code,
    string Message,
    DateTimeOffset RaisedAt,
    MachineState MachineState,
    bool RequiresRehome,
    DateTimeOffset? AcknowledgedAt = null)
{
    public bool IsAcknowledged => AcknowledgedAt.HasValue;

    public AlarmRecord Acknowledge(DateTimeOffset timestamp) =>
        this with { AcknowledgedAt = timestamp };
}
