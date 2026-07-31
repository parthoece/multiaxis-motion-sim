namespace MotionControl.Domain;

public sealed class MachineStateMachine
{
    private static readonly IReadOnlyDictionary<MachineState, HashSet<MachineState>> Allowed =
        new Dictionary<MachineState, HashSet<MachineState>>
        {
            [MachineState.Off] = [MachineState.Initializing, MachineState.Faulted],
            [MachineState.Initializing] = [MachineState.NotHomed, MachineState.Faulted, MachineState.Off],
            [MachineState.NotHomed] = [MachineState.Homing, MachineState.Faulted, MachineState.Off],
            [MachineState.Homing] = [MachineState.Ready, MachineState.Faulted, MachineState.Off],
            [MachineState.Ready] =
            [
                MachineState.Manual,
                MachineState.Automatic,
                MachineState.Faulted,
                MachineState.Off,
            ],
            [MachineState.Manual] = [MachineState.Ready, MachineState.Faulted, MachineState.Off],
            [MachineState.Automatic] =
            [
                MachineState.Ready,
                MachineState.Paused,
                MachineState.Faulted,
                MachineState.Off,
            ],
            [MachineState.Paused] =
            [
                MachineState.Automatic,
                MachineState.Ready,
                MachineState.Faulted,
                MachineState.Off,
            ],
            [MachineState.Faulted] = [MachineState.Recovering, MachineState.Off],
            [MachineState.Recovering] =
            [
                MachineState.NotHomed,
                MachineState.Ready,
                MachineState.Faulted,
                MachineState.Off,
            ],
        };

    public MachineStateMachine(MachineState initialState = MachineState.Off)
    {
        Current = initialState;
    }

    public MachineState Current { get; private set; }

    public bool CanTransitionTo(MachineState target) =>
        Allowed.TryGetValue(Current, out var targets) && targets.Contains(target);

    public StateTransition TransitionTo(
        MachineState target,
        string reason,
        DateTimeOffset timestamp)
    {
        if (!CanTransitionTo(target))
        {
            throw new DomainException(
                $"Transition from {Current} to {target} is not allowed.");
        }

        var transition = new StateTransition(Current, target, reason, timestamp);
        Current = target;
        return transition;
    }

    public StateTransition EnterFault(string reason, DateTimeOffset timestamp)
    {
        if (Current == MachineState.Faulted)
        {
            return new StateTransition(Current, Current, reason, timestamp);
        }

        if (!CanTransitionTo(MachineState.Faulted))
        {
            throw new DomainException(
                $"State {Current} cannot transition to Faulted.");
        }

        return TransitionTo(MachineState.Faulted, reason, timestamp);
    }
}
