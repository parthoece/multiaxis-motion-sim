using MotionControl.Domain;

namespace MotionControl.Domain.Tests;

public sealed class MachineStateMachineTests
{
    [Fact]
    public void NormalInitializationAndHomingPathIsAllowed()
    {
        var machine = new MachineStateMachine();
        var now = DateTimeOffset.UtcNow;

        machine.TransitionTo(MachineState.Initializing, "start", now);
        machine.TransitionTo(MachineState.NotHomed, "initialized", now);
        machine.TransitionTo(MachineState.Homing, "home", now);
        machine.TransitionTo(MachineState.Ready, "ready", now);

        Assert.Equal(MachineState.Ready, machine.Current);
    }

    [Fact]
    public void AutomaticModeCannotStartBeforeHoming()
    {
        var machine = new MachineStateMachine();
        var now = DateTimeOffset.UtcNow;

        machine.TransitionTo(MachineState.Initializing, "start", now);
        machine.TransitionTo(MachineState.NotHomed, "initialized", now);

        Assert.Throws<DomainException>(
            () => machine.TransitionTo(MachineState.Automatic, "invalid", now));
    }

    [Fact]
    public void StartupFailureCanEnterFaulted()
    {
        var machine = new MachineStateMachine();

        machine.EnterFault(
            "Storage initialization failed.",
            DateTimeOffset.UtcNow);

        Assert.Equal(MachineState.Faulted, machine.Current);
    }

    [Fact]
    public void FaultRecoveryCanRequireRehoming()
    {
        var machine = new MachineStateMachine(MachineState.Ready);
        var now = DateTimeOffset.UtcNow;

        machine.EnterFault("probe timeout", now);
        machine.TransitionTo(MachineState.Recovering, "reset", now);
        machine.TransitionTo(MachineState.NotHomed, "rehome required", now);

        Assert.Equal(MachineState.NotHomed, machine.Current);
    }
}
