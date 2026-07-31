namespace MotionControl.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
