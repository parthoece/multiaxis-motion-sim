using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class MachineLifecycleService
{
    private readonly MachineControlContext _context;

    public MachineLifecycleService(MachineControlContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _context.EnsureState(MachineState.Off);

        try
        {
            await _context.OperationsStore.InitializeAsync(cancellationToken);

            await _context.TransitionAsync(
                MachineState.Initializing,
                "Application initialization started.",
                cancellationToken);

            await _context.MotionController.InitializeAsync(cancellationToken);
            await _context.ValidatePermissivesAsync(
                requirePart: false,
                cancellationToken);

            await _context.TransitionAsync(
                MachineState.NotHomed,
                "Controller initialized; homing is required.",
                cancellationToken);

            await _context.UpdatePlcOutputsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await _context.TryStopMotionAsync(CancellationToken.None);
            await _context.EnterFaultAsync(
                new MotionControlException(
                    FaultCode.OperationCancelled,
                    "Machine initialization was cancelled."),
                CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await _context.TryStopMotionAsync(CancellationToken.None);
            await _context.EnterFaultAsync(
                MachineControlContext.ConvertException(exception),
                CancellationToken.None);
            throw;
        }
    }

    public async Task HomeAllAsync(CancellationToken cancellationToken)
    {
        _context.EnsureState(MachineState.NotHomed);

        try
        {
            await _context.ValidatePermissivesAsync(
                requirePart: false,
                cancellationToken);

            await _context.TransitionAsync(
                MachineState.Homing,
                "Homing started.",
                cancellationToken);

            await _context.MotionController.HomeAllAsync(cancellationToken);

            await _context.TransitionAsync(
                MachineState.Ready,
                "All axes homed.",
                cancellationToken);

            await _context.UpdatePlcOutputsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await _context.TryStopMotionAsync(CancellationToken.None);
            await _context.EnterFaultAsync(
                new MotionControlException(
                    FaultCode.OperationCancelled,
                    "Homing was cancelled."),
                CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await _context.TryStopMotionAsync(CancellationToken.None);
            await _context.EnterFaultAsync(
                MachineControlContext.ConvertException(
                    exception,
                    FaultCode.HomingFailed),
                CancellationToken.None);
            throw;
        }
    }

    public async Task ResetFaultAsync(CancellationToken cancellationToken)
    {
        _context.EnsureState(MachineState.Faulted);

        var alarm = _context.Runtime.ActiveAlarm ??
            throw new DomainException(
                "Faulted state requires an active alarm.");

        try
        {
            await _context.TransitionAsync(
                MachineState.Recovering,
                $"Recovery started for {alarm.Code}.",
                cancellationToken);

            await _context.ValidatePermissivesAsync(
                requirePart: false,
                cancellationToken);

            var acknowledged =
                _context.Runtime.AcknowledgeActiveAlarm(_context.Clock.UtcNow);

            await _context.OperationsStore.SaveAlarmAsync(
                acknowledged,
                cancellationToken);

            var target =
                _context.RecoveryPolicy.GetRecoveryTarget(alarm.Code);

            await _context.TransitionAsync(
                target,
                $"Fault {alarm.Code} reset.",
                cancellationToken);

            _context.Runtime.ClearActiveAlarm();
            await _context.UpdatePlcOutputsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await _context.EnterFaultAsync(
                new MotionControlException(
                    FaultCode.OperationCancelled,
                    "Fault recovery was cancelled."),
                CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            var fault = MachineControlContext.ConvertException(exception);
            await _context.EnterFaultAsync(fault, CancellationToken.None);
            throw;
        }
    }
}
