namespace MotionControl.Application;

public sealed class NullOperationEventLog : IOperationEventLog
{
    public Task WriteAsync(
        string level,
        string eventName,
        object context,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
