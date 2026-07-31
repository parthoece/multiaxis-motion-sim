using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class MachineCommandGate
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        if (!await _gate.WaitAsync(TimeSpan.Zero, cancellationToken))
        {
            throw new DomainException(
                "Another machine command is already active.");
        }

        try
        {
            await operation(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        if (!await _gate.WaitAsync(TimeSpan.Zero, cancellationToken))
        {
            throw new DomainException(
                "Another machine command is already active.");
        }

        try
        {
            return await operation(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
