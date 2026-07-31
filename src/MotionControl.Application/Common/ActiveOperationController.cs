using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class ActiveOperationController
{
    private readonly object _sync = new();

    private CancellationTokenSource? _activeCancellation;
    private TaskCompletionSource? _activeCompletion;

    public bool HasActiveOperation
    {
        get
        {
            lock (_sync)
            {
                return _activeCancellation is not null;
            }
        }
    }

    public ActiveOperationScope Begin(CancellationToken externalToken)
    {
        lock (_sync)
        {
            if (_activeCancellation is not null)
            {
                throw new DomainException(
                    "Another machine operation is already active.");
            }

            _activeCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _activeCompletion = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

            return new ActiveOperationScope(
                this,
                _activeCancellation,
                _activeCompletion);
        }
    }

    public bool RequestCancellation()
    {
        CancellationTokenSource? cancellation;

        lock (_sync)
        {
            cancellation = _activeCancellation;
        }

        if (cancellation is null)
        {
            return false;
        }

        try
        {
            cancellation.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public async Task WaitForCompletionAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        Task completion;

        lock (_sync)
        {
            completion = _activeCompletion?.Task ?? Task.CompletedTask;
        }

        await completion.WaitAsync(timeout, cancellationToken);
    }

    private void Complete(
        CancellationTokenSource cancellation,
        TaskCompletionSource completion)
    {
        lock (_sync)
        {
            if (!ReferenceEquals(_activeCancellation, cancellation))
            {
                return;
            }

            _activeCancellation = null;
            _activeCompletion = null;
        }

        completion.TrySetResult();
        cancellation.Dispose();
    }

    internal sealed class ActiveOperationScope : IDisposable
    {
        private readonly ActiveOperationController _owner;
        private readonly CancellationTokenSource _cancellation;
        private readonly TaskCompletionSource _completion;
        private int _disposed;

        internal ActiveOperationScope(
            ActiveOperationController owner,
            CancellationTokenSource cancellation,
            TaskCompletionSource completion)
        {
            _owner = owner;
            _cancellation = cancellation;
            _completion = completion;
        }

        public CancellationToken Token => _cancellation.Token;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _owner.Complete(_cancellation, _completion);
        }
    }
}
