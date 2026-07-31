using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class MachineControlContext
{
    private readonly object _warningSync = new();
    private readonly List<OperationalWarning> _warnings = [];

    public MachineControlContext(
        IMotionController motionController,
        IPlcGateway plcGateway,
        IOperationsStore operationsStore,
        IOperationEventLog eventLog,
        IClock clock,
        MachineRuntime runtime,
        FaultRecoveryPolicy recoveryPolicy,
        PlcOutputPolicy plcOutputPolicy)
    {
        MotionController = motionController;
        PlcGateway = plcGateway;
        OperationsStore = operationsStore;
        EventLog = eventLog;
        Clock = clock;
        Runtime = runtime;
        RecoveryPolicy = recoveryPolicy;
        PlcOutputPolicy = plcOutputPolicy;
    }

    public IMotionController MotionController { get; }

    public IPlcGateway PlcGateway { get; }

    public IOperationsStore OperationsStore { get; }

    public IOperationEventLog EventLog { get; }

    public IClock Clock { get; }

    public MachineRuntime Runtime { get; }

    public FaultRecoveryPolicy RecoveryPolicy { get; }

    public PlcOutputPolicy PlcOutputPolicy { get; }

    public IReadOnlyList<OperationalWarning> Warnings
    {
        get
        {
            lock (_warningSync)
            {
                return _warnings.ToArray();
            }
        }
    }

    public void EnsureState(MachineState required)
    {
        if (Runtime.CurrentState != required)
        {
            throw new DomainException(
                $"Operation requires state {required}; " +
                $"current state is {Runtime.CurrentState}.");
        }
    }

    public async Task<SafetyInputs> ValidatePermissivesAsync(
        bool requirePart,
        CancellationToken cancellationToken)
    {
        var inputs = await PlcGateway.ReadInputsAsync(cancellationToken);
        Runtime.SetSafetyInputs(inputs);

        var blockingFaults = inputs.GetBlockingFaults(requirePart);
        if (blockingFaults.Count > 0)
        {
            var first = blockingFaults[0];
            throw new MotionControlException(
                first,
                $"Machine permissive failed: {first}.");
        }

        return inputs;
    }

    public async Task<StateTransition> TransitionAsync(
        MachineState target,
        string reason,
        CancellationToken cancellationToken)
    {
        var transition = Runtime.TransitionTo(target, reason, Clock.UtcNow);
        await OperationsStore.SaveStateTransitionAsync(
            transition,
            cancellationToken);
        await TryWriteEventAsync(
            "Information",
            "MachineStateTransition",
            transition,
            cancellationToken);

        return transition;
    }

    public async Task UpdatePlcOutputsAsync(
        CancellationToken cancellationToken)
    {
        var outputs = PlcOutputPolicy.GetOutputs(Runtime.CurrentState);
        await PlcGateway.WriteOutputsAsync(outputs, cancellationToken);
    }

    public async Task EnterFaultAsync(
        MotionControlException primaryFault,
        CancellationToken cancellationToken)
    {
        var transition = Runtime.EnterFault(
            primaryFault.Message,
            Clock.UtcNow);

        var alarm = new AlarmRecord(
            Guid.NewGuid(),
            primaryFault.FaultCode,
            primaryFault.Message,
            Clock.UtcNow,
            MachineState.Faulted,
            RecoveryPolicy.RequiresRehome(primaryFault.FaultCode));

        Runtime.SetActiveAlarm(alarm);

        if (transition.From != transition.To)
        {
            await TrySecondaryActionAsync(
                "PersistFaultTransition",
                () => OperationsStore.SaveStateTransitionAsync(
                    transition,
                    cancellationToken),
                cancellationToken);
        }

        await TrySecondaryActionAsync(
            "PersistPrimaryAlarm",
            () => OperationsStore.SaveAlarmAsync(alarm, cancellationToken),
            cancellationToken);

        await TrySecondaryActionAsync(
            "WritePrimaryFaultEvent",
            () => EventLog.WriteAsync(
                "Error",
                "MachineFaulted",
                alarm,
                cancellationToken),
            cancellationToken);

        await TrySecondaryActionAsync(
            "WriteFaultPlcOutputs",
            () => UpdatePlcOutputsAsync(cancellationToken),
            cancellationToken);
    }

    public async Task TryStopMotionAsync(
        CancellationToken cancellationToken)
    {
        await TrySecondaryActionAsync(
            "StopMotion",
            () => MotionController.StopAsync(cancellationToken),
            cancellationToken);
    }

    public async Task<SafetyInputs> TryReadSafetyInputsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var inputs = await PlcGateway.ReadInputsAsync(cancellationToken);
            Runtime.SetSafetyInputs(inputs);
            return inputs;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            RecordWarning("ReadSafetyInputs", exception);
            return Runtime.LastSafetyInputs;
        }
    }

    public async Task TryWriteEventAsync(
        string level,
        string eventName,
        object context,
        CancellationToken cancellationToken)
    {
        await TrySecondaryActionAsync(
            eventName,
            () => EventLog.WriteAsync(
                level,
                eventName,
                context,
                cancellationToken),
            cancellationToken);
    }

    public void RecordWarning(string eventName, Exception exception)
    {
        lock (_warningSync)
        {
            _warnings.Add(new OperationalWarning(
                eventName,
                exception.Message,
                Clock.UtcNow));
        }
    }

    public static MotionControlException ConvertException(
        Exception exception,
        FaultCode fallback = FaultCode.UnexpectedSoftwareError) =>
        exception as MotionControlException ??
        new MotionControlException(fallback, exception.Message);

    private async Task TrySecondaryActionAsync(
        string eventName,
        Func<Task> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            RecordWarning(eventName, exception);
        }
    }
}
