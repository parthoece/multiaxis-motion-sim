namespace MotionControl.Domain;

public sealed record MachineSnapshot(
    MachineState State,
    MachineMode Mode,
    AxisVector Position,
    bool IsInitialized,
    bool IsHomed,
    bool IsMoving,
    SafetyInputs SafetyInputs,
    FaultCode ActiveFault,
    DateTimeOffset Timestamp);
