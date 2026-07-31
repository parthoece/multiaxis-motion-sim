using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using MotionControl.Application;
using MotionControl.Domain;

namespace MotionControl.GrblHal;

public sealed class GrblHalMotionController :
    IMotionController,
    IAsyncDisposable
{
    private static readonly AxisVector Minimum = new(0, 0, 0);
    private static readonly AxisVector Maximum = new(500, 400, 150);

    private readonly GrblHalOptions _options;
    private readonly SemaphoreSlim _commandGate = new(1, 1);
    private readonly SemaphoreSlim _statusGate = new(1, 1);
    private readonly Channel<string> _responses =
        Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
            });
    private readonly object _sync = new();

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _readerCancellation;
    private Task? _readerTask;

    private TaskCompletionSource<ControllerStatus>? _statusWaiter;
    private TaskCompletionSource<AxisVector>? _probeWaiter;

    private ControllerStatus _latestStatus =
        new(ControllerState.Unknown, AxisVector.Origin, string.Empty);

    private bool _initialized;
    private bool _homed;

    public GrblHalMotionController(GrblHalOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new ArgumentException(
                "The grblHAL host cannot be empty.",
                nameof(options));
        }

        if (options.Port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The grblHAL port must be between 1 and 65535.");
        }

        _options = options;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await ConnectAsync(cancellationToken);

            var status = await RequestStatusAsync(cancellationToken);

            if (status.State == ControllerState.Alarm &&
                _options.UnlockAlarmOnInitialize)
            {
                await ExecuteLineAsync("$X", cancellationToken);
                status = await RequestStatusAsync(cancellationToken);
            }

            await ExecuteLineAsync("G21", cancellationToken);
            await ExecuteLineAsync("G90", cancellationToken);

            lock (_sync)
            {
                _initialized = true;
                _homed = false;
                _latestStatus = status;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new MotionControlException(
                FaultCode.MotionControllerUnavailable,
                $"Could not initialize grblHAL: {exception.Message}");
        }
    }

    public async Task HomeAllAsync(
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var status = await RequestStatusAsync(cancellationToken);

            if (status.State == ControllerState.Alarm &&
                _options.UnlockAlarmOnInitialize)
            {
                await ExecuteLineAsync("$X", cancellationToken);
            }

            if (_options.SoftwareOnlyInputs)
            {
                // Hardware-free mode has no physical home switches.
                // Keep the controller in absolute millimetre mode and
                // treat the current simulator position as the home reference.
                await ExecuteLineAsync(
                    "G21",
                    cancellationToken);

                await ExecuteLineAsync(
                    "G90",
                    cancellationToken);

                _ = await RequestStatusAsync(
                    cancellationToken);
            }
            else
            {
                // Real controller mode. This requires configured home inputs.
                await ExecuteLineAsync("$H", cancellationToken);

                var finalStatus =
                    await RequestStatusAsync(cancellationToken);

                if (finalStatus.State == ControllerState.Alarm)
                {
                    throw new GrblHalProtocolException(
                        "grblHAL entered Alarm during homing.");
                }
            }

            lock (_sync)
            {
                _homed = true;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            lock (_sync)
            {
                _homed = false;
            }

            throw new MotionControlException(
                FaultCode.HomingFailed,
                $"grblHAL homing failed: {exception.Message}");
        }
    }

    public async Task MoveAbsoluteAsync(
        AxisVector target,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken)
    {
        EnsureReadyForMotion();
        ValidateTarget(target);

        if (velocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(velocityMillimetersPerSecond),
                "Velocity must be greater than zero.");
        }

        var feedMillimetersPerMinute =
            velocityMillimetersPerSecond * 60.0;

        var command = string.Format(
            CultureInfo.InvariantCulture,
            "G21 G90 G1 X{0:0.###} Y{1:0.###} Z{2:0.###} F{3:0.###}",
            target.X,
            target.Y,
            target.Z,
            feedMillimetersPerMinute);

        try
        {
            await ExecuteMotionLineAsync(
                command,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (GrblHalProtocolException exception)
            when (exception.AlarmCode == 2)
        {
            throw new MotionControlException(
                FaultCode.MotionLimitExceeded,
                $"grblHAL rejected target {target}: {exception.Message}");
        }
        catch (Exception exception)
        {
            throw new MotionControlException(
                FaultCode.MotionControllerUnavailable,
                $"grblHAL movement failed: {exception.Message}");
        }
    }

    public async Task<double> ProbeZAsync(
        double targetZMillimeters,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken)
    {
        EnsureReadyForMotion();

        if (velocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(velocityMillimetersPerSecond),
                "Velocity must be greater than zero.");
        }

        var feedMillimetersPerMinute =
            velocityMillimetersPerSecond * 60.0;

        if (_options.SoftwareOnlyInputs)
        {
            var surface = _options.SimulatedProbeSurfaceZ;

            if (targetZMillimeters > surface)
            {
                throw new MotionControlException(
                    FaultCode.ProbeTimeout,
                    "The probe target does not cross the simulated surface.");
            }

            // Hardware-free mode: grblHAL plans and executes the Z movement.
            // Probe contact itself is modeled because no electrical probe exists.
            var probeCommand = string.Format(
                CultureInfo.InvariantCulture,
                "G21 G90 G1 Z{0:0.####} F{1:0.###}",
                surface,
                feedMillimetersPerMinute);

            await ExecuteMotionLineAsync(
                probeCommand,
                cancellationToken);

            return surface;
        }

        TaskCompletionSource<AxisVector> probeWaiter;

        lock (_sync)
        {
            if (_probeWaiter is not null)
            {
                throw new InvalidOperationException(
                    "A grblHAL probe operation is already active.");
            }

            probeWaiter =
                new TaskCompletionSource<AxisVector>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            _probeWaiter = probeWaiter;
        }

        try
        {
            var probeCommand = string.Format(
                CultureInfo.InvariantCulture,
                "G21 G90 G38.2 Z{0:0.####} F{1:0.###}",
                targetZMillimeters,
                feedMillimetersPerMinute);

            await ExecuteLineAsync(
                probeCommand,
                cancellationToken);

            var probePosition = await probeWaiter.Task.WaitAsync(
                _options.MotionTimeout,
                cancellationToken);

            return probePosition.Z;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (GrblHalProtocolException exception)
            when (exception.AlarmCode == 4)
        {
            throw new MotionControlException(
                FaultCode.ProbeAlreadyActive,
                "grblHAL reported that the probe was already active.");
        }
        catch (GrblHalProtocolException exception)
            when (exception.AlarmCode == 5)
        {
            throw new MotionControlException(
                FaultCode.ProbeTimeout,
                "grblHAL did not detect probe contact before the target.");
        }
        catch (Exception exception)
        {
            throw new MotionControlException(
                FaultCode.ProbeTimeout,
                $"grblHAL probing failed: {exception.Message}");
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_probeWaiter, probeWaiter))
                {
                    _probeWaiter = null;
                }
            }
        }
    }

    public async Task StopAsync(
        CancellationToken cancellationToken)
    {
        bool initialized;

        lock (_sync)
        {
            initialized = _initialized;
        }

        if (!initialized || _stream is null)
        {
            return;
        }

        // Feed hold is a real-time byte and therefore has no line terminator.
        await SendRealtimeAsync(
            (byte)'!',
            cancellationToken);

        try
        {
            await WaitForAnyStateAsync(
                [ControllerState.Hold, ControllerState.Idle],
                TimeSpan.FromSeconds(5),
                cancellationToken);
        }
        finally
        {
            // Abort buffered motion. A reset during movement invalidates position,
            // so this backend deliberately requires homing again.
            await SendRealtimeAsync(
                0x18,
                cancellationToken);

            await Task.Delay(
                TimeSpan.FromMilliseconds(250),
                cancellationToken);

            lock (_sync)
            {
                _homed = false;
            }
        }
    }

    public async Task<MachineSnapshot> GetSnapshotAsync(
        CancellationToken cancellationToken)
    {
        bool initialized;
        bool homed;

        lock (_sync)
        {
            initialized = _initialized;
            homed = _homed;
        }

        if (!initialized || _stream is null)
        {
            return new MachineSnapshot(
                State: MachineState.Off,
                Mode: MachineMode.None,
                Position: AxisVector.Origin,
                IsInitialized: false,
                IsHomed: false,
                IsMoving: false,
                SafetyInputs: SafetyInputs.Ready,
                ActiveFault: FaultCode.None,
                Timestamp: DateTimeOffset.UtcNow);
        }

        try
        {
            var status = await RequestStatusAsync(cancellationToken);

            return new MachineSnapshot(
                State: MachineState.Off,
                Mode: MachineMode.None,
                Position: status.Position,
                IsInitialized: true,
                IsHomed: homed,
                IsMoving: IsMovingState(status.State),
                SafetyInputs: SafetyInputs.Ready,
                ActiveFault: FaultCode.None,
                Timestamp: DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new MotionControlException(
                FaultCode.MotionControllerUnavailable,
                $"Could not read grblHAL status: {exception.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_readerCancellation is not null)
        {
            await _readerCancellation.CancelAsync();
        }

        _tcpClient?.Close();

        if (_readerTask is not null)
        {
            try
            {
                await _readerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during disposal.
            }
            catch (IOException)
            {
                // The socket was closed during disposal.
            }
        }

        _writer?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _readerCancellation?.Dispose();
        _commandGate.Dispose();
        _statusGate.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task ConnectAsync(
        CancellationToken cancellationToken)
    {
        if (_tcpClient?.Connected == true)
        {
            return;
        }

        var client = new TcpClient();

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            cancellationToken);

        var stream = client.GetStream();

        var reader = new StreamReader(
            stream,
            Encoding.ASCII,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        var writer = new StreamWriter(
            stream,
            Encoding.ASCII,
            bufferSize: 4096,
            leaveOpen: true)
        {
            AutoFlush = true,
            NewLine = "\n",
        };

        var readerCancellation =
            new CancellationTokenSource();

        _tcpClient = client;
        _stream = stream;
        _reader = reader;
        _writer = writer;
        _readerCancellation = readerCancellation;

        _readerTask = Task.Run(
            () => ReadLoopAsync(readerCancellation.Token),
            CancellationToken.None);
    }

    private async Task ReadLoopAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await _reader!.ReadLineAsync(
                    cancellationToken);

                if (line is null)
                {
                    break;
                }

                line = line.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                if (TryParseStatus(line, out var status))
                {
                    TaskCompletionSource<ControllerStatus>? waiter;

                    lock (_sync)
                    {
                        _latestStatus = status;
                        waiter = _statusWaiter;
                    }

                    waiter?.TrySetResult(status);
                    continue;
                }

                if (TryParseProbe(line, out var probePosition))
                {
                    TaskCompletionSource<AxisVector>? waiter;

                    lock (_sync)
                    {
                        waiter = _probeWaiter;
                    }

                    waiter?.TrySetResult(probePosition);
                    continue;
                }

                await _responses.Writer.WriteAsync(
                    line,
                    cancellationToken);
            }

            _responses.Writer.TryComplete();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _responses.Writer.TryComplete();
        }
        catch (Exception exception)
        {
            _responses.Writer.TryComplete(exception);

            lock (_sync)
            {
                _statusWaiter?.TrySetException(exception);
                _probeWaiter?.TrySetException(exception);
            }
        }
    }

    private async Task<IReadOnlyList<string>> ExecuteLineAsync(
        string command,
        CancellationToken cancellationToken)
    {
        await _commandGate.WaitAsync(cancellationToken);

        try
        {
            EnsureConnected();

            await _writer!.WriteLineAsync(
                command.AsMemory(),
                cancellationToken);

            var messages = new List<string>();

            using var timeoutCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutCancellation.CancelAfter(
                _options.CommandTimeout);

            while (true)
            {
                var line = await _responses.Reader.ReadAsync(
                    timeoutCancellation.Token);

                if (string.Equals(
                        line,
                        "ok",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return messages;
                }

                if (line.StartsWith(
                        "error:",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new GrblHalProtocolException(line);
                }

                if (line.StartsWith(
                        "ALARM:",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new GrblHalProtocolException(
                        line,
                        ParseNumericSuffix(line));
                }

                messages.Add(line);
            }
        }
        finally
        {
            _commandGate.Release();
        }
    }

    private async Task ExecuteMotionLineAsync(
        string command,
        CancellationToken cancellationToken)
    {
        await ExecuteLineAsync(command, cancellationToken);
        await WaitForMotionCompletionAsync(cancellationToken);
    }

    private async Task<ControllerStatus> RequestStatusAsync(
        CancellationToken cancellationToken)
    {
        await _statusGate.WaitAsync(cancellationToken);

        TaskCompletionSource<ControllerStatus>? waiter = null;

        try
        {
            EnsureConnected();

            waiter =
                new TaskCompletionSource<ControllerStatus>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_sync)
            {
                _statusWaiter = waiter;
            }

            await SendRealtimeAsync(
                (byte)'?',
                cancellationToken);

            return await waiter.Task.WaitAsync(
                _options.CommandTimeout,
                cancellationToken);
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_statusWaiter, waiter))
                {
                    _statusWaiter = null;
                }
            }

            _statusGate.Release();
        }
    }

    private async Task SendRealtimeAsync(
        byte command,
        CancellationToken cancellationToken)
    {
        EnsureConnected();

        await _stream!.WriteAsync(
            new[] { command },
            cancellationToken);

        await _stream.FlushAsync(cancellationToken);
    }

    private async Task WaitForMotionCompletionAsync(
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var sawMotion = false;

        while (stopwatch.Elapsed < _options.MotionTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status =
                await RequestStatusAsync(cancellationToken);

            if (status.State == ControllerState.Alarm)
            {
                throw new GrblHalProtocolException(
                    "grblHAL entered Alarm while executing motion.");
            }

            if (IsMovingState(status.State))
            {
                sawMotion = true;
            }
            else if (status.State == ControllerState.Idle &&
                     (sawMotion ||
                      stopwatch.Elapsed >=
                      TimeSpan.FromMilliseconds(150)))
            {
                return;
            }

            await Task.Delay(
                _options.StatusPollInterval,
                cancellationToken);
        }

        throw new TimeoutException(
            "Timed out waiting for grblHAL motion to finish.");
    }

    private async Task WaitForAnyStateAsync(
        IReadOnlyCollection<ControllerState> expectedStates,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status =
                await RequestStatusAsync(cancellationToken);

            if (expectedStates.Contains(status.State))
            {
                return;
            }

            await Task.Delay(
                _options.StatusPollInterval,
                cancellationToken);
        }

        throw new TimeoutException(
            "Timed out waiting for the requested grblHAL state.");
    }

    private void EnsureConnected()
    {
        if (_tcpClient?.Connected != true ||
            _stream is null ||
            _reader is null ||
            _writer is null)
        {
            throw new InvalidOperationException(
                "The grblHAL TCP connection is not open.");
        }
    }

    private void EnsureInitialized()
    {
        lock (_sync)
        {
            if (!_initialized)
            {
                throw new MotionControlException(
                    FaultCode.MotionControllerUnavailable,
                    "The grblHAL controller has not been initialized.");
            }
        }
    }

    private void EnsureReadyForMotion()
    {
        EnsureInitialized();

        lock (_sync)
        {
            if (!_homed)
            {
                throw new MotionControlException(
                    FaultCode.HomingFailed,
                    "Motion is prohibited until grblHAL has been homed.");
            }
        }
    }

    private static void ValidateTarget(AxisVector target)
    {
        if (target.X < Minimum.X || target.X > Maximum.X ||
            target.Y < Minimum.Y || target.Y > Maximum.Y ||
            target.Z < Minimum.Z || target.Z > Maximum.Z)
        {
            throw new MotionControlException(
                FaultCode.MotionLimitExceeded,
                $"Target {target} is outside the XYZ software limits.");
        }
    }

    private static bool IsMovingState(
        ControllerState state) =>
        state is
            ControllerState.Run or
            ControllerState.Jog or
            ControllerState.Home;

    private static bool TryParseStatus(
        string line,
        out ControllerStatus status)
    {
        status = default;

        if (line.Length < 3 ||
            line[0] != '<' ||
            line[^1] != '>')
        {
            return false;
        }

        var fields = line[1..^1].Split('|');

        if (fields.Length == 0)
        {
            return false;
        }

        var state = ParseState(fields[0]);
        var position = AxisVector.Origin;

        foreach (var field in fields.Skip(1))
        {
            if (!field.StartsWith(
                    "MPos:",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var coordinates =
                field["MPos:".Length..].Split(',');

            if (coordinates.Length >= 3 &&
                TryParseDouble(coordinates[0], out var x) &&
                TryParseDouble(coordinates[1], out var y) &&
                TryParseDouble(coordinates[2], out var z))
            {
                position = new AxisVector(x, y, z);
            }
        }

        status = new ControllerStatus(
            state,
            position,
            line);

        return true;
    }

    private static bool TryParseProbe(
        string line,
        out AxisVector position)
    {
        position = AxisVector.Origin;

        if (!line.StartsWith(
                "[PRB:",
                StringComparison.OrdinalIgnoreCase) ||
            !line.EndsWith(']'))
        {
            return false;
        }

        var payload = line[5..^1];
        var separatorIndex = payload.LastIndexOf(':');

        if (separatorIndex >= 0)
        {
            payload = payload[..separatorIndex];
        }

        var coordinates = payload.Split(',');

        return coordinates.Length >= 3 &&
               TryParseDouble(coordinates[0], out var x) &&
               TryParseDouble(coordinates[1], out var y) &&
               TryParseDouble(coordinates[2], out var z) &&
               AssignProbePosition(x, y, z, out position);
    }

    private static bool AssignProbePosition(
        double x,
        double y,
        double z,
        out AxisVector position)
    {
        position = new AxisVector(x, y, z);
        return true;
    }

    private static ControllerState ParseState(
        string value)
    {
        var state = value.Split(':', 2)[0];

        return state.ToUpperInvariant() switch
        {
            "IDLE" => ControllerState.Idle,
            "RUN" => ControllerState.Run,
            "HOLD" => ControllerState.Hold,
            "JOG" => ControllerState.Jog,
            "ALARM" => ControllerState.Alarm,
            "DOOR" => ControllerState.Door,
            "CHECK" => ControllerState.Check,
            "HOME" => ControllerState.Home,
            "SLEEP" => ControllerState.Sleep,
            _ => ControllerState.Unknown,
        };
    }

    private static bool TryParseDouble(
        string value,
        out double result) =>
        double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out result);

    private static int? ParseNumericSuffix(
        string value)
    {
        var separatorIndex = value.IndexOf(':');

        return separatorIndex >= 0 &&
               int.TryParse(
                   value[(separatorIndex + 1)..],
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out var result)
            ? result
            : null;
    }

    private enum ControllerState
    {
        Unknown,
        Idle,
        Run,
        Hold,
        Jog,
        Alarm,
        Door,
        Check,
        Home,
        Sleep,
    }

    private readonly record struct ControllerStatus(
        ControllerState State,
        AxisVector Position,
        string Raw);

    private sealed class GrblHalProtocolException :
        Exception
    {
        public GrblHalProtocolException(
            string message,
            int? alarmCode = null)
            : base(message)
        {
            AlarmCode = alarmCode;
        }

        public int? AlarmCode { get; }
    }
}
