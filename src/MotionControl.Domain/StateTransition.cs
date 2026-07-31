namespace MotionControl.Domain;

public sealed record StateTransition(
    MachineState From,
    MachineState To,
    string Reason,
    DateTimeOffset Timestamp);
