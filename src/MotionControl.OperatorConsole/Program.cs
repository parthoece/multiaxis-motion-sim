using MotionControl.Application;
using MotionControl.Domain;
using MotionControl.Persistence;
using MotionControl.Simulation;

var scenarioName =
    args.Length > 0
        ? args[0].Trim().ToLowerInvariant()
        : "normal";

var scenario = new SimulationScenario
{
    ActiveFault = scenarioName switch
    {
        "normal" => SimulationFault.None,
        "operator-stop" => SimulationFault.None,
        "probe-timeout" => SimulationFault.ProbeTimeout,
        "probe-active" => SimulationFault.ProbeAlreadyActive,
        "estop" => SimulationFault.EmergencyStopActive,
        "door-open" => SimulationFault.DoorOpen,
        "part-missing" => SimulationFault.PartMissing,
        "plc-loss" => SimulationFault.PlcCommunicationLost,
        "homing-failure" => SimulationFault.HomingFailed,
        "positive-limit" => SimulationFault.PositiveLimit,
        "out-of-tolerance" =>
            SimulationFault.OutOfTolerancePart,
        _ => throw new ArgumentException(
            $"Unknown scenario '{scenarioName}'. " +
            "Use normal, operator-stop, probe-timeout, probe-active, " +
            "estop, door-open, part-missing, plc-loss, " +
            "homing-failure, positive-limit, or out-of-tolerance."),
    },
    TimeScale = scenarioName == "operator-stop"
        ? 0.1
        : 0.001,
};

var runtimeDirectory = Path.Combine(
    AppContext.BaseDirectory,
    "..",
    "..",
    "..",
    "..",
    "..",
    ".runtime");

Directory.CreateDirectory(runtimeDirectory);

var coordinator = new MachineCoordinator(
    new DeterministicMotionController(scenario),
    new VirtualPlcGateway(scenario),
    new SqliteOperationsStore(
        Path.Combine(runtimeDirectory, "operations.db")),
    new JsonLineEventLog(
        Path.Combine(runtimeDirectory, "events.jsonl")),
    new SystemClock(),
    new RecipeValidator());

using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

Console.WriteLine(
    "Virtual Multi-Axis Motion Control Platform");
Console.WriteLine($"Scenario: {scenarioName}");
Console.WriteLine();

try
{
    await coordinator.InitializeAsync(cancellation.Token);
    PrintSnapshot(
        await coordinator.GetSnapshotAsync(cancellation.Token));

    await coordinator.HomeAllAsync(cancellation.Token);
    PrintSnapshot(
        await coordinator.GetSnapshotAsync(cancellation.Token));

    if (scenarioName == "operator-stop")
    {
        await RunOperatorStopScenarioAsync(
            coordinator,
            cancellation.Token);
    }
    else
    {
        var report = await coordinator.RunInspectionAsync(
            InspectionRecipe.Demo,
            cancellation.Token);

        PrintReport(report);
        PrintSnapshot(
            await coordinator.GetSnapshotAsync(cancellation.Token));
    }

    PrintWarnings(coordinator.OperationalWarnings);
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"{exception.GetType().Name}: {exception.Message}");

    var alarm = coordinator.ActiveAlarm;
    if (alarm is not null)
    {
        Console.Error.WriteLine(
            $"Alarm {alarm.Code}: " +
            $"requiresRehome={alarm.RequiresRehome}");
    }

    PrintWarnings(coordinator.OperationalWarnings);
    Environment.ExitCode = 1;
}

static async Task RunOperatorStopScenarioAsync(
    MachineCoordinator coordinator,
    CancellationToken cancellationToken)
{
    Console.WriteLine(
        "Starting inspection and requesting operator stop.");

    var cycleTask = coordinator.RunInspectionAsync(
        InspectionRecipe.Demo,
        cancellationToken);

    await WaitForStateAsync(
        coordinator,
        MachineState.Automatic,
        TimeSpan.FromSeconds(3),
        cancellationToken);

    await coordinator.StopAsync(cancellationToken);

    try
    {
        await cycleTask;
        throw new InvalidOperationException(
            "The cycle completed even though operator stop was requested.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine(
            "Active cycle cancellation confirmed.");
    }

    var alarm = coordinator.ActiveAlarm ??
        throw new InvalidOperationException(
            "Operator stop did not create an active alarm.");

    if (alarm.Code != FaultCode.OperationCancelled)
    {
        throw new InvalidOperationException(
            $"Expected OperationCancelled, found {alarm.Code}.");
    }

    PrintSnapshot(
        await coordinator.GetSnapshotAsync(cancellationToken));

    await coordinator.ResetFaultAsync(cancellationToken);

    Console.WriteLine(
        "Operator-stop recovery completed.");
    PrintSnapshot(
        await coordinator.GetSnapshotAsync(cancellationToken));
}

static async Task WaitForStateAsync(
    MachineCoordinator coordinator,
    MachineState expected,
    TimeSpan timeout,
    CancellationToken cancellationToken)
{
    using var timeoutCancellation =
        new CancellationTokenSource(timeout);
    using var linkedCancellation =
        CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);

    while (coordinator.CurrentState != expected)
    {
        linkedCancellation.Token.ThrowIfCancellationRequested();
        await Task.Delay(5, linkedCancellation.Token);
    }
}

static void PrintReport(CycleReport report)
{
    Console.WriteLine(
        $"Cycle {report.CycleId}: " +
        $"{(report.Passed ? "PASS" : "FAIL")} " +
        $"in {report.Duration.TotalMilliseconds:F0} ms");

    foreach (var measurement in report.Measurements)
    {
        Console.WriteLine(
            $"  {measurement.PointName}: " +
            $"{measurement.MeasuredMillimeters:F4} mm " +
            $"error=" +
            $"{measurement.ErrorMillimeters:+0.0000;-0.0000;0.0000} " +
            $"{(measurement.Passed ? "PASS" : "FAIL")}");
    }
}

static void PrintWarnings(
    IReadOnlyList<OperationalWarning> warnings)
{
    if (warnings.Count == 0)
    {
        return;
    }

    Console.WriteLine();
    Console.WriteLine(
        $"Operational warnings: {warnings.Count}");

    foreach (var warning in warnings)
    {
        Console.WriteLine(
            $"  {warning.Timestamp:O} " +
            $"{warning.EventName}: {warning.Message}");
    }
}

static void PrintSnapshot(MachineSnapshot snapshot)
{
    Console.WriteLine(
        $"State={snapshot.State,-11} " +
        $"X={snapshot.Position.X,8:F3} " +
        $"Y={snapshot.Position.Y,8:F3} " +
        $"Z={snapshot.Position.Z,8:F3} " +
        $"Moving={snapshot.IsMoving,-5} " +
        $"Homed={snapshot.IsHomed}");
}
