namespace MotionControl.Domain;

public sealed record PlcOutputs(
    bool MachineReady,
    bool CycleActive,
    bool CycleComplete,
    bool AlarmActive,
    bool GreenLight,
    bool YellowLight,
    bool RedLight)
{
    public static PlcOutputs Off => new(false, false, false, false, false, false, false);
}
