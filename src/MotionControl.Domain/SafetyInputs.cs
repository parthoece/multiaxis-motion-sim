namespace MotionControl.Domain;

public sealed record SafetyInputs(
    bool EmergencyStopReset,
    bool DoorClosed,
    bool PartPresent,
    bool AirPressureReady)
{
    public static SafetyInputs Ready => new(
        EmergencyStopReset: true,
        DoorClosed: true,
        PartPresent: true,
        AirPressureReady: true);

    public IReadOnlyList<FaultCode> GetBlockingFaults(bool requirePart)
    {
        var faults = new List<FaultCode>();

        if (!EmergencyStopReset)
        {
            faults.Add(FaultCode.EmergencyStopActive);
        }

        if (!DoorClosed)
        {
            faults.Add(FaultCode.DoorOpen);
        }

        if (!AirPressureReady)
        {
            faults.Add(FaultCode.AirPressureNotReady);
        }

        if (requirePart && !PartPresent)
        {
            faults.Add(FaultCode.PartMissing);
        }

        return faults;
    }
}
