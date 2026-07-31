using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MotionControl.Application;
using MotionControl.Domain;
using MotionControl.Simulation;

namespace MotionControl.Hmi.Wpf;

public sealed class MainViewModel :
    INotifyPropertyChanged,
    IAsyncDisposable
{
    private static readonly TimeSpan StatusInterval =
        TimeSpan.FromMilliseconds(100);

    private readonly MachineCoordinator _coordinator;
    private readonly SimulationScenario _scenario;
    private readonly CancellationTokenSource _statusCancellation = new();

    private readonly AsyncCommand _initializeCommand;
    private readonly AsyncCommand _homeCommand;
    private readonly AsyncCommand _runCycleCommand;
    private readonly AsyncCommand _resetFaultCommand;
    private readonly AsyncCommand _stopCommand;
    private readonly AsyncCommand _injectProbeTimeoutCommand;
    private readonly AsyncCommand _clearFaultInjectionCommand;

    private Task? _statusTask;
    private MachineState _currentState = MachineState.Off;
    private string _state = "Off";
    private string _statusMessage = "Ready to initialize.";
    private string _activeAlarm = "None";
    private string _safetySummary = "E-stop reset · Door closed · Part present · Air ready";
    private bool _isMoving;
    private int _warningCount;
    private double _x;
    private double _y;
    private double _z;

    public MainViewModel(
        MachineCoordinator coordinator,
        SimulationScenario scenario)
    {
        _coordinator = coordinator;
        _scenario = scenario;

        _initializeCommand = new AsyncCommand(
            () => ExecuteAndRefreshAsync(
                token => _coordinator.InitializeAsync(token)),
            () => _currentState == MachineState.Off);

        _homeCommand = new AsyncCommand(
            () => ExecuteAndRefreshAsync(
                token => _coordinator.HomeAllAsync(token)),
            () => _currentState == MachineState.NotHomed);

        _runCycleCommand = new AsyncCommand(
            RunCycleAsync,
            () => _currentState == MachineState.Ready);

        _resetFaultCommand = new AsyncCommand(
            () => ExecuteAndRefreshAsync(
                token => _coordinator.ResetFaultAsync(token)),
            () => _currentState == MachineState.Faulted);

        _stopCommand = new AsyncCommand(
            () => ExecuteAndRefreshAsync(
                token => _coordinator.StopAsync(token)),
            CanStop);

        _injectProbeTimeoutCommand = new AsyncCommand(
            () =>
            {
                _scenario.ActiveFault = SimulationFault.ProbeTimeout;
                StatusMessage = "Probe-timeout fault armed.";
                return Task.CompletedTask;
            },
            CanChangeScenario);

        _clearFaultInjectionCommand = new AsyncCommand(
            () =>
            {
                _scenario.ActiveFault = SimulationFault.None;
                StatusMessage = "Fault injection cleared.";
                return Task.CompletedTask;
            },
            CanChangeScenario);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand InitializeCommand => _initializeCommand;

    public ICommand HomeCommand => _homeCommand;

    public ICommand RunCycleCommand => _runCycleCommand;

    public ICommand ResetFaultCommand => _resetFaultCommand;

    public ICommand StopCommand => _stopCommand;

    public ICommand InjectProbeTimeoutCommand =>
        _injectProbeTimeoutCommand;

    public ICommand ClearFaultInjectionCommand =>
        _clearFaultInjectionCommand;

    public ObservableCollection<MeasurementRow> Measurements { get; } = [];

    public string State
    {
        get => _state;
        private set => SetField(ref _state, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public string ActiveAlarm
    {
        get => _activeAlarm;
        private set => SetField(ref _activeAlarm, value);
    }

    public string SafetySummary
    {
        get => _safetySummary;
        private set => SetField(ref _safetySummary, value);
    }

    public bool IsMoving
    {
        get => _isMoving;
        private set => SetField(ref _isMoving, value);
    }

    public int WarningCount
    {
        get => _warningCount;
        private set => SetField(ref _warningCount, value);
    }

    public double X
    {
        get => _x;
        private set => SetField(ref _x, value);
    }

    public double Y
    {
        get => _y;
        private set => SetField(ref _y, value);
    }

    public double Z
    {
        get => _z;
        private set => SetField(ref _z, value);
    }

    public void StartStatusMonitoring()
    {
        _statusTask ??= MonitorStatusAsync(_statusCancellation.Token);
    }

    public async ValueTask DisposeAsync()
    {
        _statusCancellation.Cancel();

        if (_statusTask is not null)
        {
            try
            {
                await _statusTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during application shutdown.
            }
        }

        _statusCancellation.Dispose();
    }

    private async Task RunCycleAsync()
    {
        await ExecuteAndRefreshAsync(async token =>
        {
            var report = await _coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                token);

            Measurements.Clear();
            foreach (var measurement in report.Measurements)
            {
                Measurements.Add(new MeasurementRow(
                    measurement.PointName,
                    measurement.NominalMillimeters,
                    measurement.MeasuredMillimeters,
                    measurement.ErrorMillimeters,
                    measurement.Passed));
            }

            StatusMessage =
                $"Cycle {report.CycleId}: " +
                $"{(report.Passed ? "PASS" : "FAIL")}";
        });
    }

    private async Task ExecuteAndRefreshAsync(
        Func<CancellationToken, Task> operation)
    {
        try
        {
            using var cancellation = new CancellationTokenSource(
                TimeSpan.FromSeconds(30));

            await operation(cancellation.Token);

            if (!StatusMessage.StartsWith(
                "Cycle",
                StringComparison.Ordinal))
            {
                StatusMessage = "Command completed.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "The operation was cancelled.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
        finally
        {
            await RefreshAsync();
        }
    }

    private async Task MonitorStatusAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var snapshot in
                _coordinator.ObserveStatusAsync(
                    StatusInterval,
                    cancellationToken))
            {
                ApplySnapshot(snapshot);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during application shutdown.
        }
        catch (Exception exception)
        {
            StatusMessage =
                $"Status monitoring stopped: {exception.Message}";
        }
    }

    private async Task RefreshAsync()
    {
        var snapshot = await _coordinator.GetSnapshotAsync(
            CancellationToken.None);
        ApplySnapshot(snapshot);
    }

    private void ApplySnapshot(MachineSnapshot snapshot)
    {
        _currentState = snapshot.State;
        State = snapshot.State.ToString();
        X = snapshot.Position.X;
        Y = snapshot.Position.Y;
        Z = snapshot.Position.Z;
        IsMoving = snapshot.IsMoving;
        ActiveAlarm =
            _coordinator.ActiveAlarm?.Code.ToString() ?? "None";
        WarningCount = _coordinator.OperationalWarnings.Count;
        SafetySummary = FormatSafety(snapshot.SafetyInputs);

        RefreshCommandStates();
    }

    private bool CanStop() =>
        IsMoving ||
        _currentState is MachineState.Homing
            or MachineState.Automatic
            or MachineState.Manual
            or MachineState.Paused;

    private bool CanChangeScenario() =>
        _currentState is not MachineState.Homing
            and not MachineState.Automatic
            and not MachineState.Recovering;

    private void RefreshCommandStates()
    {
        _initializeCommand.NotifyCanExecuteChanged();
        _homeCommand.NotifyCanExecuteChanged();
        _runCycleCommand.NotifyCanExecuteChanged();
        _resetFaultCommand.NotifyCanExecuteChanged();
        _stopCommand.NotifyCanExecuteChanged();
        _injectProbeTimeoutCommand.NotifyCanExecuteChanged();
        _clearFaultInjectionCommand.NotifyCanExecuteChanged();
    }

    private static string FormatSafety(SafetyInputs inputs)
    {
        static string Mark(bool value, string ready, string blocked) =>
            value ? ready : blocked;

        return string.Join(
            " · ",
            Mark(
                inputs.EmergencyStopReset,
                "E-stop reset",
                "E-stop ACTIVE"),
            Mark(
                inputs.DoorClosed,
                "Door closed",
                "Door OPEN"),
            Mark(
                inputs.PartPresent,
                "Part present",
                "Part MISSING"),
            Mark(
                inputs.AirPressureReady,
                "Air ready",
                "Air NOT READY"));
    }

    private void SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
