using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class MachineStopService
{
    private static readonly TimeSpan StopConfirmationTimeout =
        TimeSpan.FromSeconds(5);

    private readonly IMotionController _motionController;
    private readonly ActiveOperationController _activeOperationController;
    private readonly MachineControlContext _context;

    public MachineStopService(
        IMotionController motionController,
        ActiveOperationController activeOperationController,
        MachineControlContext context)
    {
        _motionController = motionController;
        _activeOperationController = activeOperationController;
        _context = context;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var cancellationRequested =
            _activeOperationController.RequestCancellation();

        try
        {
            await _motionController.StopAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _context.RecordWarning("OperatorStopMotion", exception);
            throw;
        }

        if (!cancellationRequested)
        {
            return;
        }

        try
        {
            await _activeOperationController.WaitForCompletionAsync(
                StopConfirmationTimeout,
                cancellationToken);
        }
        catch (TimeoutException exception)
        {
            _context.RecordWarning("OperatorStopConfirmationTimeout", exception);
            throw new MotionControlException(
                FaultCode.MotionControllerUnavailable,
                "The active operation did not confirm stop within five seconds.");
        }
    }
}
