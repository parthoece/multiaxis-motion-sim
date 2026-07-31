using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class PlcOutputPolicy
{
    public PlcOutputs GetOutputs(MachineState state) => state switch
    {
        MachineState.Ready => new PlcOutputs(
            MachineReady: true,
            CycleActive: false,
            CycleComplete: false,
            AlarmActive: false,
            GreenLight: true,
            YellowLight: false,
            RedLight: false),
        MachineState.Automatic => new PlcOutputs(
            MachineReady: false,
            CycleActive: true,
            CycleComplete: false,
            AlarmActive: false,
            GreenLight: false,
            YellowLight: true,
            RedLight: false),
        MachineState.Faulted => new PlcOutputs(
            MachineReady: false,
            CycleActive: false,
            CycleComplete: false,
            AlarmActive: true,
            GreenLight: false,
            YellowLight: false,
            RedLight: true),
        _ => PlcOutputs.Off,
    };
}
