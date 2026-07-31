using Microsoft.Data.Sqlite;
using MotionControl.Application;
using MotionControl.Domain;
using MotionControl.Persistence;
using MotionControl.Simulation;

namespace MotionControl.IntegrationTests;

public sealed class SqliteOperationsStoreTests
{
    [Fact]
    public async Task CompletedCycleIsStoredWithMeasurements()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"multiaxis-motion-sim-{Guid.NewGuid():N}.db");

        try
        {
            var scenario = new SimulationScenario();
            var store = new SqliteOperationsStore(databasePath);
            var coordinator = new MachineCoordinator(
                new DeterministicMotionController(scenario),
                new VirtualPlcGateway(scenario),
                store,
                new NullOperationEventLog(),
                new SystemClock(),
                new RecipeValidator());

            await coordinator.InitializeAsync(CancellationToken.None);
            await coordinator.HomeAllAsync(CancellationToken.None);
            await coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                CancellationToken.None);

            await using var connection = new SqliteConnection(
                $"Data Source={databasePath}");
            await connection.OpenAsync();

            var cycleCommand = connection.CreateCommand();
            cycleCommand.CommandText = "SELECT COUNT(*) FROM cycles;";
            var cycleCount = Convert.ToInt32(
                await cycleCommand.ExecuteScalarAsync());

            var measurementCommand = connection.CreateCommand();
            measurementCommand.CommandText = "SELECT COUNT(*) FROM measurements;";
            var measurementCount = Convert.ToInt32(
                await measurementCommand.ExecuteScalarAsync());

            Assert.Equal(1, cycleCount);
            Assert.Equal(5, measurementCount);
        }
        finally
        {
            File.Delete(databasePath);
        }
    }
}
