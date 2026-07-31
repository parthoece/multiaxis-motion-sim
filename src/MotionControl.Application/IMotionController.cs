using MotionControl.Domain;

namespace MotionControl.Application;

public interface IMotionController
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task HomeAllAsync(CancellationToken cancellationToken);

    Task MoveAbsoluteAsync(
        AxisVector target,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken);

    Task<double> ProbeZAsync(
        double targetZMillimeters,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    Task<MachineSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}
