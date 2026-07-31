using MotionControl.Domain;

namespace MotionControl.Application;

public sealed class InMemoryOperationsStore : IOperationsStore
{
    public List<StateTransition> StateTransitions { get; } = [];

    public List<AlarmRecord> Alarms { get; } = [];

    public List<CycleReport> Cycles { get; } = [];

    public Task InitializeAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task SaveStateTransitionAsync(
        StateTransition transition,
        CancellationToken cancellationToken)
    {
        StateTransitions.Add(transition);
        return Task.CompletedTask;
    }

    public Task SaveAlarmAsync(
        AlarmRecord alarm,
        CancellationToken cancellationToken)
    {
        Alarms.Add(alarm);
        return Task.CompletedTask;
    }

    public Task SaveCycleAsync(
        CycleReport report,
        CancellationToken cancellationToken)
    {
        Cycles.Add(report);
        return Task.CompletedTask;
    }
}
