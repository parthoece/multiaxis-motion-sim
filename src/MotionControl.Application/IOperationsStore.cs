using MotionControl.Domain;

namespace MotionControl.Application;

public interface IOperationsStore
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task SaveStateTransitionAsync(
        StateTransition transition,
        CancellationToken cancellationToken);

    Task SaveAlarmAsync(
        AlarmRecord alarm,
        CancellationToken cancellationToken);

    Task SaveCycleAsync(
        CycleReport report,
        CancellationToken cancellationToken);
}
