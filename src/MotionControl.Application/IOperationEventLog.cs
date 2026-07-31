namespace MotionControl.Application;

public interface IOperationEventLog
{
    Task WriteAsync(
        string level,
        string eventName,
        object context,
        CancellationToken cancellationToken);
}
