using MotionControl.Domain;

namespace MotionControl.Application;

public sealed class MachineCoordinator
{
    private readonly MachineRuntime _runtime;
    private readonly MachineCommandGate _commandGate;
    private readonly ActiveOperationController _activeOperationController;
    private readonly MachineLifecycleService _lifecycleService;
    private readonly InspectionCycleService _inspectionCycleService;
    private readonly MachineStatusService _statusService;
    private readonly MachineStopService _stopService;
    private readonly MachineControlContext _context;

    public MachineCoordinator(
        IMotionController motionController,
        IPlcGateway plcGateway,
        IOperationsStore operationsStore,
        IOperationEventLog eventLog,
        IClock clock,
        RecipeValidator recipeValidator)
    {
        _runtime = new MachineRuntime();
        _commandGate = new MachineCommandGate();
        _activeOperationController = new ActiveOperationController();

        _context = new MachineControlContext(
            motionController,
            plcGateway,
            operationsStore,
            eventLog,
            clock,
            _runtime,
            new FaultRecoveryPolicy(),
            new PlcOutputPolicy());

        _lifecycleService = new MachineLifecycleService(_context);
        _inspectionCycleService =
            new InspectionCycleService(_context, recipeValidator);
        _statusService = new MachineStatusService(_context);
        _stopService = new MachineStopService(
            motionController,
            _activeOperationController,
            _context);
    }

    public MachineState CurrentState => _runtime.CurrentState;

    public AlarmRecord? ActiveAlarm => _runtime.ActiveAlarm;

    public IReadOnlyList<OperationalWarning> OperationalWarnings =>
        _context.Warnings;

    public Task InitializeAsync(CancellationToken cancellationToken) =>
        ExecuteOperationAsync(
            _lifecycleService.InitializeAsync,
            cancellationToken);

    public Task HomeAllAsync(CancellationToken cancellationToken) =>
        ExecuteOperationAsync(
            _lifecycleService.HomeAllAsync,
            cancellationToken);

    public Task<CycleReport> RunInspectionAsync(
        InspectionRecipe recipe,
        CancellationToken cancellationToken) =>
        ExecuteOperationAsync(
            token => _inspectionCycleService.RunAsync(recipe, token),
            cancellationToken);

    public Task ResetFaultAsync(CancellationToken cancellationToken) =>
        ExecuteOperationAsync(
            _lifecycleService.ResetFaultAsync,
            cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) =>
        _stopService.StopAsync(cancellationToken);

    public Task<MachineSnapshot> GetSnapshotAsync(
        CancellationToken cancellationToken) =>
        _statusService.GetSnapshotAsync(cancellationToken);

    public IAsyncEnumerable<MachineSnapshot> ObserveStatusAsync(
        TimeSpan interval,
        CancellationToken cancellationToken) =>
        _statusService.ObserveAsync(interval, cancellationToken);

    private Task ExecuteOperationAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken) =>
        _commandGate.ExecuteAsync(async commandToken =>
        {
            using var activeOperation =
                _activeOperationController.Begin(commandToken);
            await operation(activeOperation.Token);
        }, cancellationToken);

    private Task<T> ExecuteOperationAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken) =>
        _commandGate.ExecuteAsync(async commandToken =>
        {
            using var activeOperation =
                _activeOperationController.Begin(commandToken);
            return await operation(activeOperation.Token);
        }, cancellationToken);
}
