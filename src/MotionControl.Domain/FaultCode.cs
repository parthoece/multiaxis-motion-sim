namespace MotionControl.Domain;

public enum FaultCode
{
    None = 0,
    EmergencyStopActive = 1,
    DoorOpen = 2,
    PartMissing = 3,
    AirPressureNotReady = 4,
    MotionLimitExceeded = 100,
    MotionControllerUnavailable = 101,
    HomingFailed = 102,
    ProbeTimeout = 200,
    ProbeAlreadyActive = 201,
    PlcCommunicationLost = 300,
    InvalidRecipe = 400,
    OperationCancelled = 500,
    UnexpectedSoftwareError = 900,
}
