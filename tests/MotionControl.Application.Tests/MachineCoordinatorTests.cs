using MotionControl.Application;
using MotionControl.Domain;
using MotionControl.Simulation;

namespace MotionControl.Application.Tests;

public sealed class MachineCoordinatorTests
{
    [Fact]
    public async Task NormalCycleReturnsToReadyAndPersistsAReport()
    {
        var scenario = new SimulationScenario();
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);
        var report = await coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            CancellationToken.None);

        Assert.Equal(MachineState.Ready, coordinator.CurrentState);
        Assert.Single(store.Cycles);
        Assert.Equal(5, report.Measurements.Count);
        Assert.True(report.Passed);
    }

    [Fact]
    public async Task OutOfToleranceScenarioCompletesWithFailedReport()
    {
        var scenario = new SimulationScenario
        {
            ActiveFault = SimulationFault.OutOfTolerancePart,
        };
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);
        var report = await coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            CancellationToken.None);

        Assert.Equal(MachineState.Ready, coordinator.CurrentState);
        Assert.False(report.Passed);
        Assert.Contains(report.Measurements, measurement => !measurement.Passed);
    }

    [Fact]
    public async Task ProbeTimeoutMovesMachineToFaultedAndRecordsAlarm()
    {
        var scenario = new SimulationScenario
        {
            ActiveFault = SimulationFault.ProbeTimeout,
        };
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        var exception = await Assert.ThrowsAsync<MotionControlException>(
            () => coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                CancellationToken.None));

        Assert.Equal(FaultCode.ProbeTimeout, exception.FaultCode);
        Assert.Equal(MachineState.Faulted, coordinator.CurrentState);
        Assert.Contains(
            store.Alarms,
            alarm => alarm.Code == FaultCode.ProbeTimeout);
        Assert.True(coordinator.ActiveAlarm!.RequiresRehome);
    }

    [Fact]
    public async Task ProbeTimeoutResetReturnsToNotHomed()
    {
        var scenario = new SimulationScenario
        {
            ActiveFault = SimulationFault.ProbeTimeout,
        };
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        await Assert.ThrowsAsync<MotionControlException>(
            () => coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                CancellationToken.None));

        scenario.ActiveFault = SimulationFault.None;
        await coordinator.ResetFaultAsync(CancellationToken.None);

        Assert.Equal(MachineState.NotHomed, coordinator.CurrentState);
        Assert.Null(coordinator.ActiveAlarm);
    }

    [Fact]
    public async Task MissingPartBlocksCycleWithoutInvalidatingHome()
    {
        var scenario = new SimulationScenario
        {
            ActiveFault = SimulationFault.PartMissing,
        };
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        var exception = await Assert.ThrowsAsync<MotionControlException>(
            () => coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                CancellationToken.None));

        Assert.Equal(FaultCode.PartMissing, exception.FaultCode);
        Assert.Equal(MachineState.Faulted, coordinator.CurrentState);
        Assert.False(coordinator.ActiveAlarm!.RequiresRehome);

        scenario.ActiveFault = SimulationFault.None;
        await coordinator.ResetFaultAsync(CancellationToken.None);

        Assert.Equal(MachineState.Ready, coordinator.CurrentState);
    }

    [Fact]
    public async Task InvalidRecipeIsRejectedWithoutFaultingMachine()
    {
        var scenario = new SimulationScenario();
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);
        var invalidRecipe = InspectionRecipe.Demo with
        {
            InspectionPoints =
            [
                new InspectionPoint(
                    "OUTSIDE",
                    900,
                    20,
                    10,
                    9.95,
                    10.05),
            ],
        };

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        var exception = await Assert.ThrowsAsync<MotionControlException>(
            () => coordinator.RunInspectionAsync(
                invalidRecipe,
                CancellationToken.None));

        Assert.Equal(FaultCode.InvalidRecipe, exception.FaultCode);
        Assert.Equal(MachineState.Ready, coordinator.CurrentState);
        Assert.Null(coordinator.ActiveAlarm);
    }

    [Fact]
    public async Task ControllerInitializationFailureRecoversToOff()
    {
        var motionController =
            new FailOnceInitializationMotionController();
        var coordinator = new MachineCoordinator(
            motionController,
            new ReadyPlcGateway(),
            new InMemoryOperationsStore(),
            new NullOperationEventLog(),
            new SystemClock(),
            new RecipeValidator());

        var exception = await Assert.ThrowsAsync<MotionControlException>(
            () => coordinator.InitializeAsync(
                CancellationToken.None));

        Assert.Equal(
            FaultCode.MotionControllerUnavailable,
            exception.FaultCode);
        Assert.Equal(MachineState.Faulted, coordinator.CurrentState);

        await coordinator.ResetFaultAsync(CancellationToken.None);
        Assert.Equal(MachineState.Off, coordinator.CurrentState);

        await coordinator.InitializeAsync(CancellationToken.None);
        Assert.Equal(MachineState.NotHomed, coordinator.CurrentState);
    }


    [Fact]
    public async Task OperatorStopCancelsCycleAndRecoveryReturnsToReady()
    {
        var scenario = new SimulationScenario
        {
            TimeScale = 0.1,
        };
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        var cycleTask = coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            CancellationToken.None);

        await WaitForStateAsync(
            coordinator,
            MachineState.Automatic,
            TimeSpan.FromSeconds(2));

        await coordinator.StopAsync(CancellationToken.None);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => cycleTask);

        Assert.Equal(MachineState.Faulted, coordinator.CurrentState);
        Assert.Equal(
            FaultCode.OperationCancelled,
            coordinator.ActiveAlarm?.Code);
        Assert.False(coordinator.ActiveAlarm!.RequiresRehome);

        await coordinator.ResetFaultAsync(CancellationToken.None);
        Assert.Equal(MachineState.Ready, coordinator.CurrentState);
    }

    [Fact]
    public async Task OverlappingAutomaticCommandsAreRejected()
    {
        var scenario = new SimulationScenario
        {
            TimeScale = 0.1,
        };
        var store = new InMemoryOperationsStore();
        var coordinator = CreateCoordinator(scenario, store);

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        var firstCycle = coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            CancellationToken.None);

        await WaitForStateAsync(
            coordinator,
            MachineState.Automatic,
            TimeSpan.FromSeconds(2));

        await Assert.ThrowsAsync<DomainException>(
            () => coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                CancellationToken.None));

        await firstCycle;
    }

    [Fact]
    public async Task DiagnosticLogFailureDoesNotStopNormalCycle()
    {
        var scenario = new SimulationScenario();
        var store = new InMemoryOperationsStore();
        var coordinator = new MachineCoordinator(
            new DeterministicMotionController(scenario),
            new VirtualPlcGateway(scenario),
            store,
            new AlwaysFailingEventLog(),
            new SystemClock(),
            new RecipeValidator());

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);
        var report = await coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            CancellationToken.None);

        Assert.True(report.Passed);
        Assert.Equal(MachineState.Ready, coordinator.CurrentState);
        Assert.NotEmpty(coordinator.OperationalWarnings);
    }


    [Fact]
    public async Task SecondaryPlcFailureDoesNotReplaceProbeTimeout()
    {
        var scenario = new SimulationScenario
        {
            ActiveFault = SimulationFault.ProbeTimeout,
        };
        var store = new InMemoryOperationsStore();
        var gateway = new AlarmOutputFailingPlcGateway();
        var coordinator = new MachineCoordinator(
            new DeterministicMotionController(scenario),
            gateway,
            store,
            new NullOperationEventLog(),
            new SystemClock(),
            new RecipeValidator());

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        var exception = await Assert.ThrowsAsync<MotionControlException>(
            () => coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                CancellationToken.None));

        Assert.Equal(FaultCode.ProbeTimeout, exception.FaultCode);
        Assert.Equal(
            FaultCode.ProbeTimeout,
            coordinator.ActiveAlarm?.Code);
        Assert.Contains(
            coordinator.OperationalWarnings,
            warning =>
                warning.EventName == "WriteFaultPlcOutputs");
    }

    [Fact]
    public async Task StatusStreamPublishesAutomaticMotion()
    {
        var scenario = new SimulationScenario
        {
            TimeScale = 0.05,
        };
        var coordinator = CreateCoordinator(
            scenario,
            new InMemoryOperationsStore());

        await coordinator.InitializeAsync(CancellationToken.None);
        await coordinator.HomeAllAsync(CancellationToken.None);

        using var cancellation =
            new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var observedAutomatic = false;
        var cycleTask = coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            CancellationToken.None);

        await foreach (var snapshot in coordinator.ObserveStatusAsync(
            TimeSpan.FromMilliseconds(10),
            cancellation.Token))
        {
            if (snapshot.State == MachineState.Automatic &&
                snapshot.IsMoving)
            {
                observedAutomatic = true;
                break;
            }
        }

        await cycleTask;
        Assert.True(observedAutomatic);
    }

    private static MachineCoordinator CreateCoordinator(
        SimulationScenario scenario,
        InMemoryOperationsStore store) =>
        new(
            new DeterministicMotionController(scenario),
            new VirtualPlcGateway(scenario),
            store,
            new NullOperationEventLog(),
            new SystemClock(),
            new RecipeValidator());

    private static async Task WaitForStateAsync(
        MachineCoordinator coordinator,
        MachineState expected,
        TimeSpan timeout)
    {
        using var cancellation =
            new CancellationTokenSource(timeout);

        while (coordinator.CurrentState != expected)
        {
            cancellation.Token.ThrowIfCancellationRequested();
            await Task.Delay(5, cancellation.Token);
        }
    }

    private sealed class ReadyPlcGateway : IPlcGateway
    {
        public Task<SafetyInputs> ReadInputsAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(SafetyInputs.Ready);

        public Task WriteOutputsAsync(
            PlcOutputs outputs,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FailOnceInitializationMotionController :
        IMotionController
    {
        private bool _failed;

        public Task InitializeAsync(
            CancellationToken cancellationToken)
        {
            if (!_failed)
            {
                _failed = true;
                throw new MotionControlException(
                    FaultCode.MotionControllerUnavailable,
                    "The controller is unavailable.");
            }

            return Task.CompletedTask;
        }

        public Task HomeAllAsync(
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task MoveAbsoluteAsync(
            AxisVector target,
            double velocityMillimetersPerSecond,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<double> ProbeZAsync(
            double targetZMillimeters,
            double velocityMillimetersPerSecond,
            CancellationToken cancellationToken) =>
            Task.FromResult(10.0);

        public Task StopAsync(
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<MachineSnapshot> GetSnapshotAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(new MachineSnapshot(
                State: MachineState.Off,
                Mode: MachineMode.None,
                Position: AxisVector.Origin,
                IsInitialized: _failed,
                IsHomed: false,
                IsMoving: false,
                SafetyInputs: SafetyInputs.Ready,
                ActiveFault: FaultCode.None,
                Timestamp: DateTimeOffset.UtcNow));
    }


    private sealed class AlwaysFailingEventLog : IOperationEventLog
    {
        public Task WriteAsync(
            string level,
            string eventName,
            object context,
            CancellationToken cancellationToken) =>
            throw new IOException("The diagnostic event sink is unavailable.");
    }


    private sealed class AlarmOutputFailingPlcGateway : IPlcGateway
    {
        public Task<SafetyInputs> ReadInputsAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(SafetyInputs.Ready);

        public Task WriteOutputsAsync(
            PlcOutputs outputs,
            CancellationToken cancellationToken)
        {
            if (outputs.AlarmActive)
            {
                throw new MotionControlException(
                    FaultCode.PlcCommunicationLost,
                    "PLC alarm output write failed.");
            }

            return Task.CompletedTask;
        }
    }
}
