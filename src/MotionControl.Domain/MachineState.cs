namespace MotionControl.Domain;

public enum MachineState
{
    Off,
    Initializing,
    NotHomed,
    Homing,
    Ready,
    Manual,
    Automatic,
    Paused,
    Faulted,
    Recovering,
}
