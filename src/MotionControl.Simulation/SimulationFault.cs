namespace MotionControl.Simulation;

public enum SimulationFault
{
    None,
    EmergencyStopActive,
    DoorOpen,
    PartMissing,
    AirPressureNotReady,
    PlcCommunicationLost,
    HomingFailed,
    ProbeTimeout,
    ProbeAlreadyActive,
    PositiveLimit,
    OutOfTolerancePart,
}
