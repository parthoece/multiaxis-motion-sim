using System;
using System.IO;
using System.Windows;
using MotionControl.Application;
using MotionControl.Domain;
using MotionControl.GrblHal;
using MotionControl.Persistence;
using MotionControl.Simulation;

namespace MotionControl.Hmi.Wpf;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var runtimeDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "multiaxis-motion-sim");

        Directory.CreateDirectory(runtimeDirectory);

        var scenario = new SimulationScenario();

        IMotionController motionController =
            CreateMotionController(scenario);

        var coordinator = new MachineCoordinator(
            motionController,
            new VirtualPlcGateway(scenario),
            new SqliteOperationsStore(
                Path.Combine(runtimeDirectory, "operations.db")),
            new JsonLineEventLog(
                Path.Combine(runtimeDirectory, "events.jsonl")),
            new SystemClock(),
            new RecipeValidator());

        var viewModel = new MainViewModel(
            coordinator,
            scenario);

        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Closed += async (_, _) =>
        {
            await viewModel.DisposeAsync();

            if (motionController is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        };

        MainWindow = window;
        window.Show();

        viewModel.StartStatusMonitoring();
    }

    private static IMotionController CreateMotionController(
        SimulationScenario scenario)
    {
        var backend =
            Environment.GetEnvironmentVariable("MOTION_BACKEND")
            ?? "simulation";

        if (!backend.Equals(
                "grblhal",
                StringComparison.OrdinalIgnoreCase))
        {
            return new DeterministicMotionController(scenario);
        }

        var host =
            Environment.GetEnvironmentVariable("GRBLHAL_HOST")
            ?? "127.0.0.1";

        var port =
            int.TryParse(
                Environment.GetEnvironmentVariable("GRBLHAL_PORT"),
                out var configuredPort)
                ? configuredPort
                : 23000;

        return new GrblHalMotionController(
            new GrblHalOptions
            {
                Host = host,
                Port = port,

                // Keep enabled for a completely software-only demo.
                SoftwareOnlyInputs = true,
            });
    }
}