using MotionControl.Application;
using MotionControl.Domain;

namespace MotionControl.Simulation;

public sealed class DeterministicMotionController : IMotionController
{
    private static readonly AxisVector Minimum = new(0, 0, 0);
    private static readonly AxisVector Maximum = new(500, 400, 150);

    private readonly SimulationScenario _scenario;
    private readonly object _sync = new();

    private AxisVector _position = AxisVector.Origin;
    private bool _initialized;
    private bool _homed;
    private bool _moving;

    public DeterministicMotionController(SimulationScenario scenario)
    {
        _scenario = scenario;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await DelayAsync(20, cancellationToken);
        lock (_sync)
        {
            _initialized = true;
            _homed = false;
            _position = AxisVector.Origin;
        }
    }

    public async Task HomeAllAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        if (_scenario.ActiveFault == SimulationFault.HomingFailed)
        {
            await DelayAsync(50, cancellationToken);
            throw new MotionControlException(
                FaultCode.HomingFailed,
                "The simulated Z home sensor did not activate.");
        }

        SetMoving(true);
        try
        {
            await MoveInternalAsync(
                new AxisVector(_position.X, _position.Y, 150),
                30,
                cancellationToken);
            await MoveInternalAsync(
                new AxisVector(0, _position.Y, 150),
                100,
                cancellationToken);
            await MoveInternalAsync(
                new AxisVector(0, 0, 150),
                80,
                cancellationToken);

            lock (_sync)
            {
                _homed = true;
            }
        }
        finally
        {
            SetMoving(false);
        }
    }

    public async Task MoveAbsoluteAsync(
        AxisVector target,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken)
    {
        EnsureReadyForMotion();
        ValidateTarget(target);

        if (_scenario.ActiveFault == SimulationFault.PositiveLimit && target.X > 150)
        {
            throw new MotionControlException(
                FaultCode.MotionLimitExceeded,
                "The simulated X positive limit became active.");
        }

        SetMoving(true);
        try
        {
            await MoveInternalAsync(target, velocityMillimetersPerSecond, cancellationToken);
        }
        finally
        {
            SetMoving(false);
        }
    }

    public async Task<double> ProbeZAsync(
        double targetZMillimeters,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken)
    {
        EnsureReadyForMotion();

        if (_scenario.ActiveFault == SimulationFault.ProbeAlreadyActive)
        {
            throw new MotionControlException(
                FaultCode.ProbeAlreadyActive,
                "The virtual probe input was active before the probe move.");
        }

        if (_scenario.ActiveFault == SimulationFault.ProbeTimeout)
        {
            await DelayAsync(100, cancellationToken);
            throw new MotionControlException(
                FaultCode.ProbeTimeout,
                "The virtual probe did not trigger before the target position.");
        }

        AxisVector start;
        lock (_sync)
        {
            start = _position;
        }

        var surface = CalculateSurfaceHeight(start.X, start.Y);
        if (targetZMillimeters > surface)
        {
            throw new MotionControlException(
                FaultCode.ProbeTimeout,
                "The requested probe target does not cross the virtual surface.");
        }

        SetMoving(true);
        try
        {
            await MoveInternalAsync(
                start with { Z = surface },
                velocityMillimetersPerSecond,
                cancellationToken);
            return surface;
        }
        finally
        {
            SetMoving(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        SetMoving(false);
        return Task.CompletedTask;
    }

    public Task<MachineSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            return Task.FromResult(new MachineSnapshot(
                State: MachineState.Off,
                Mode: MachineMode.None,
                Position: _position,
                IsInitialized: _initialized,
                IsHomed: _homed,
                IsMoving: _moving,
                SafetyInputs: SafetyInputs.Ready,
                ActiveFault: FaultCode.None,
                Timestamp: DateTimeOffset.UtcNow));
        }
    }

    private double CalculateSurfaceHeight(double x, double y)
    {
        var deterministicVariation =
            (Math.Sin((x + _scenario.Seed) / 37.0) +
             Math.Cos((y + _scenario.Seed) / 29.0)) * 0.008;

        var faultOffset = _scenario.ActiveFault == SimulationFault.OutOfTolerancePart
            ? 0.15
            : 0.0;

        return Math.Round(10.0 + deterministicVariation + faultOffset, 4);
    }

    private async Task MoveInternalAsync(
        AxisVector target,
        double velocityMillimetersPerSecond,
        CancellationToken cancellationToken)
    {
        if (velocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(velocityMillimetersPerSecond),
                "Velocity must be greater than zero.");
        }

        AxisVector start;
        lock (_sync)
        {
            start = _position;
        }

        var distance = Math.Sqrt(
            Math.Pow(target.X - start.X, 2) +
            Math.Pow(target.Y - start.Y, 2) +
            Math.Pow(target.Z - start.Z, 2));

        var simulatedMilliseconds = Math.Max(
            1,
            (int)Math.Ceiling(
                distance / velocityMillimetersPerSecond * 1000 * _scenario.TimeScale));

        var stepCount = Math.Clamp(simulatedMilliseconds / 10, 1, 100);
        var stepDelayMilliseconds = Math.Max(
            1,
            simulatedMilliseconds / stepCount);

        for (var step = 1; step <= stepCount; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(stepDelayMilliseconds, cancellationToken);

            var progress = (double)step / stepCount;
            var intermediate = new AxisVector(
                start.X + ((target.X - start.X) * progress),
                start.Y + ((target.Y - start.Y) * progress),
                start.Z + ((target.Z - start.Z) * progress));

            lock (_sync)
            {
                _position = intermediate;
            }
        }
    }

    private Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
    {
        var scaled = Math.Max(
            1,
            (int)Math.Ceiling(milliseconds * _scenario.TimeScale));
        return Task.Delay(scaled, cancellationToken);
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

    private void EnsureInitialized()
    {
        lock (_sync)
        {
            if (!_initialized)
            {
                throw new MotionControlException(
                    FaultCode.MotionControllerUnavailable,
                    "The motion controller has not been initialized.");
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
                    "Motion is prohibited until all axes are homed.");
            }
        }
    }

    private void SetMoving(bool value)
    {
        lock (_sync)
        {
            _moving = value;
        }
    }
}
