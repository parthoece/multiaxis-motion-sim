using System;
using System.IO;
using System.Windows;
using MotionControl.Application;
using MotionControl.Domain;
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

        var coordinator = new MachineCoordinator(
            new DeterministicMotionController(scenario),
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
        };

        MainWindow = window;
        window.Show();

        viewModel.StartStatusMonitoring();
    }
}