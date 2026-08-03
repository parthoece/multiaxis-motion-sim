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
    private readonly AsyncCommand _runGuidedDemoCommand;
    private readonly AsyncCommand _recoverToReadyCommand;
    private readonly AsyncCommand _armSelectedFaultCommand;
    private readonly AsyncCommand _clearFaultInjectionCommand;

    private Task? _statusTask;
    private MachineState _currentState = MachineState.Off;
    private string _state = "Off";
    private string _statusMessage = string.Empty;
    private string _activeAlarm = "None";
    private string _activeInjection = "None";
    private string _safetySummary = "E-stop reset | Door closed | Part present | Air ready";
    private string _nextAction = "Initialize machine.";
    private bool _isMoving;
    private int _warningCount;
    private int _totalCycles;
    private int _passedCycles;
    private int _failedCycles;
    private double _lastCycleDurationMs;
    private double _x;
    private double _y;
    private double _z;
    private SimulationFault _selectedFault = SimulationFault.ProbeTimeout;

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

        _runGuidedDemoCommand = new AsyncCommand(
            RunGuidedDemoAsync,
            CanRunGuidedDemo);

        _recoverToReadyCommand = new AsyncCommand(
            RecoverToReadyAsync,
            CanRecoverToReady);

        _armSelectedFaultCommand = new AsyncCommand(
            () =>
            {
                _scenario.ActiveFault = _selectedFault;
                ActiveInjection = _selectedFault.ToString();
                SetStatus($"Fault injection armed: {_selectedFault}.");
                return Task.CompletedTask;
            },
            CanChangeScenario);

        _clearFaultInjectionCommand = new AsyncCommand(
            () =>
            {
                _scenario.ActiveFault = SimulationFault.None;
                ActiveInjection = "None";
                SetStatus("Fault injection cleared.");
                return Task.CompletedTask;
            },
            () => CanChangeScenario() && _scenario.ActiveFault != SimulationFault.None);

        SetStatus("Ready to initialize.");
        AddEvent("HMI session started.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand InitializeCommand => _initializeCommand;

    public ICommand HomeCommand => _homeCommand;

    public ICommand RunCycleCommand => _runCycleCommand;

    public ICommand ResetFaultCommand => _resetFaultCommand;

    public ICommand StopCommand => _stopCommand;

    public ICommand RunGuidedDemoCommand => _runGuidedDemoCommand;

    public ICommand RecoverToReadyCommand => _recoverToReadyCommand;

    public ICommand ArmSelectedFaultCommand => _armSelectedFaultCommand;

    public ICommand ClearFaultInjectionCommand => _clearFaultInjectionCommand;

    public ObservableCollection<MeasurementRow> Measurements { get; } = [];

    public ObservableCollection<string> EventLog { get; } = [];

    public IReadOnlyList<SimulationFault> AvailableFaults { get; } =
        Enum.GetValues<SimulationFault>()
            .Where(fault => fault != SimulationFault.None)
            .ToArray();

    public string BackendName =>
        string.Equals(
            Environment.GetEnvironmentVariable("MOTION_BACKEND"),
            "grblhal",
            StringComparison.OrdinalIgnoreCase)
            ? "grblHAL"
            : "Deterministic Simulation";

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

    public string ActiveInjection
    {
        get => _activeInjection;
        private set => SetField(ref _activeInjection, value);
    }

    public string SafetySummary
    {
        get => _safetySummary;
        private set => SetField(ref _safetySummary, value);
    }

    public string NextAction
    {
        get => _nextAction;
        private set => SetField(ref _nextAction, value);
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

    public int TotalCycles
    {
        get => _totalCycles;
        private set => SetField(ref _totalCycles, value);
    }

    public int PassedCycles
    {
        get => _passedCycles;
        private set => SetField(ref _passedCycles, value);
    }

    public int FailedCycles
    {
        get => _failedCycles;
        private set => SetField(ref _failedCycles, value);
    }

    public double LastCycleDurationMs
    {
        get => _lastCycleDurationMs;
        private set => SetField(ref _lastCycleDurationMs, value);
    }

    public double CyclePassRatePercent =>
        TotalCycles == 0 ? 0 : (PassedCycles * 100.0) / TotalCycles;

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

    public SimulationFault SelectedFault
    {
        get => _selectedFault;
        set
        {
            if (SetField(ref _selectedFault, value))
            {
                _armSelectedFaultCommand.NotifyCanExecuteChanged();
            }
        }
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
            SetStatus("Running inspection cycle...");
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

            SetStatus(
                $"Cycle {report.CycleId}: " +
                $"{(report.Passed ? "PASS" : "FAIL")}");

            TotalCycles += 1;
            if (report.Passed)
            {
                PassedCycles += 1;
            }
            else
            {
                FailedCycles += 1;
            }

            LastCycleDurationMs = report.Duration.TotalMilliseconds;
            NotifyPropertyChanged(nameof(CyclePassRatePercent));
        });
    }

    private async Task RunGuidedDemoAsync()
    {
        try
        {
            using var cancellation = new CancellationTokenSource(
                TimeSpan.FromSeconds(60));

            SetStatus("Guided flow started.");

            if (_currentState == MachineState.Off)
            {
                await _coordinator.InitializeAsync(cancellation.Token);
                await RefreshAsync();
                AddEvent("Guided flow: Initialize completed.");
            }

            if (_currentState == MachineState.NotHomed)
            {
                await _coordinator.HomeAllAsync(cancellation.Token);
                await RefreshAsync();
                AddEvent("Guided flow: Homing completed.");
            }

            if (_currentState == MachineState.Faulted)
            {
                await _coordinator.ResetFaultAsync(cancellation.Token);
                await RefreshAsync();
                AddEvent("Guided flow: Fault reset completed.");

                if (_currentState == MachineState.NotHomed)
                {
                    await _coordinator.HomeAllAsync(cancellation.Token);
                    await RefreshAsync();
                    AddEvent("Guided flow: Rehoming completed.");
                }
            }

            if (_currentState == MachineState.Ready)
            {
                await RunCycleAsync();
            }
            else
            {
                SetStatus(
                    $"Guided flow stopped in state {_currentState}. " +
                    "Review safety inputs and active alarms.");
            }
        }
        catch (OperationCanceledException)
        {
            SetStatus("Guided flow cancelled.");
        }
        catch (Exception exception)
        {
            SetStatus($"Guided flow failed: {exception.Message}");
        }
        finally
        {
            await RefreshAsync();
        }
    }

    private async Task RecoverToReadyAsync()
    {
        await ExecuteAndRefreshAsync(async token =>
        {
            if (_currentState == MachineState.Faulted)
            {
                await _coordinator.ResetFaultAsync(token);
                await RefreshAsync();
                AddEvent("Fault reset requested.");
            }

            if (_currentState == MachineState.Off)
            {
                await _coordinator.InitializeAsync(token);
                await RefreshAsync();
                AddEvent("Initialize requested.");
            }

            if (_currentState == MachineState.NotHomed)
            {
                await _coordinator.HomeAllAsync(token);
                await RefreshAsync();
                AddEvent("Home all requested.");
            }

            SetStatus("Recovery flow completed.");
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

            if (!StatusMessage.StartsWith("Cycle", StringComparison.Ordinal))
            {
                SetStatus("Command completed.");
            }
        }
        catch (OperationCanceledException)
        {
            SetStatus("The operation was cancelled.");
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
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
            await foreach (var snapshot in _coordinator.ObserveStatusAsync(
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
            SetStatus($"Status monitoring stopped: {exception.Message}");
        }
    }

    private async Task RefreshAsync()
    {
        var snapshot = await _coordinator.GetSnapshotAsync(CancellationToken.None);
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
        ActiveAlarm = _coordinator.ActiveAlarm?.Code.ToString() ?? "None";
        WarningCount = _coordinator.OperationalWarnings.Count;
        SafetySummary = FormatSafety(snapshot.SafetyInputs);
        ActiveInjection = _scenario.ActiveFault.ToString();
        NextAction = GetNextActionHint(snapshot);

        RefreshCommandStates();
    }

    private bool CanStop() =>
        IsMoving ||
        _currentState is MachineState.Homing
            or MachineState.Automatic
            or MachineState.Manual
            or MachineState.Paused;

    private bool CanRunGuidedDemo() =>
        _currentState is not MachineState.Initializing
            and not MachineState.Homing
            and not MachineState.Recovering;

    private bool CanRecoverToReady() =>
        _currentState is MachineState.Faulted
            or MachineState.Off
            or MachineState.NotHomed;

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
        _runGuidedDemoCommand.NotifyCanExecuteChanged();
        _recoverToReadyCommand.NotifyCanExecuteChanged();
        _armSelectedFaultCommand.NotifyCanExecuteChanged();
        _clearFaultInjectionCommand.NotifyCanExecuteChanged();
    }

    private string GetNextActionHint(MachineSnapshot snapshot)
    {
        if (snapshot.State == MachineState.Off)
        {
            return "Initialize to power on the machine workflow.";
        }

        if (snapshot.State == MachineState.NotHomed)
        {
            return "Home all axes before running inspection.";
        }

        if (snapshot.State == MachineState.Ready)
        {
            return "Run inspection, or arm a fault and run fault behavior.";
        }

        if (snapshot.State == MachineState.Faulted)
        {
            return "Reset fault, then rehome if required.";
        }

        if (snapshot.State is MachineState.Automatic
            or MachineState.Homing
            or MachineState.Recovering)
        {
            return "Observe live state or press Stop if needed.";
        }

        return "Use Guided flow for the recommended path.";
    }

    private void SetStatus(string message)
    {
        StatusMessage = message;
        AddEvent(message);
    }

    private void AddEvent(string message)
    {
        var compact = message.Replace(Environment.NewLine, " ");
        var timestamped = $"{DateTime.Now:HH:mm:ss} | {compact}";
        EventLog.Insert(0, timestamped);

        while (EventLog.Count > 60)
        {
            EventLog.RemoveAt(EventLog.Count - 1);
        }
    }

    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string FormatSafety(SafetyInputs inputs)
    {
        static string Mark(bool value, string ready, string blocked) =>
            value ? ready : blocked;

        return string.Join(
            " | ",
            Mark(inputs.EmergencyStopReset, "E-stop reset", "E-stop ACTIVE"),
            Mark(inputs.DoorClosed, "Door closed", "Door OPEN"),
            Mark(inputs.PartPresent, "Part present", "Part MISSING"),
            Mark(inputs.AirPressureReady, "Air ready", "Air NOT READY"));
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
