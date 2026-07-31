using System.Runtime.CompilerServices;
using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class MachineStatusService
{
    private readonly MachineControlContext _context;

    public MachineStatusService(MachineControlContext context)
    {
        _context = context;
    }

    public async Task<MachineSnapshot> GetSnapshotAsync(
        CancellationToken cancellationToken)
    {
        var motionSnapshot =
            await _context.MotionController.GetSnapshotAsync(cancellationToken);
        var safetyInputs =
            await _context.TryReadSafetyInputsAsync(cancellationToken);

        var mode = _context.Runtime.CurrentState switch
        {
            MachineState.Manual => MachineMode.Manual,
            MachineState.Automatic or MachineState.Paused =>
                MachineMode.Automatic,
            _ => MachineMode.None,
        };

        return motionSnapshot with
        {
            State = _context.Runtime.CurrentState,
            Mode = mode,
            SafetyInputs = safetyInputs,
            ActiveFault =
                _context.Runtime.ActiveAlarm?.Code ?? FaultCode.None,
        };
    }

    public async IAsyncEnumerable<MachineSnapshot> ObserveAsync(
        TimeSpan interval,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Status interval must be greater than zero.");
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            MachineSnapshot snapshot;

            try
            {
                snapshot = await GetSnapshotAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (Exception exception)
            {
                _context.RecordWarning("ObserveMachineStatus", exception);
                await Task.Delay(interval, cancellationToken);
                continue;
            }

            yield return snapshot;
            await Task.Delay(interval, cancellationToken);
        }
    }
}
