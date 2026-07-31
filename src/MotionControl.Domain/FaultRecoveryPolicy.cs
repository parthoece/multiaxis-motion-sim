namespace MotionControl.Domain;

public sealed class FaultRecoveryPolicy
{
    public MachineState GetRecoveryTarget(FaultCode faultCode) =>
        faultCode switch
        {
            FaultCode.MotionControllerUnavailable =>
                MachineState.Off,
            FaultCode.UnexpectedSoftwareError =>
                MachineState.Off,

            FaultCode.PartMissing =>
                MachineState.Ready,
            FaultCode.AirPressureNotReady =>
                MachineState.Ready,
            FaultCode.InvalidRecipe =>
                MachineState.Ready,
            FaultCode.OperationCancelled =>
                MachineState.Ready,

            FaultCode.None =>
                MachineState.Ready,

            _ =>
                MachineState.NotHomed,
        };

    public bool RequiresRehome(FaultCode faultCode) =>
        GetRecoveryTarget(faultCode) == MachineState.NotHomed;
}
