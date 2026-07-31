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
        var cancellationToken =
            TestContext.Current.CancellationToken;

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

            await coordinator.InitializeAsync(cancellationToken);
            await coordinator.HomeAllAsync(cancellationToken);

            await coordinator.RunInspectionAsync(
                InspectionRecipe.Demo,
                cancellationToken);

            var connectionString =
                new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    ForeignKeys = true,
                }.ToString();

            await using (var connection =
                         new SqliteConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using var cycleCommand = connection.CreateCommand();
                cycleCommand.CommandText =
                    "SELECT COUNT(*) FROM cycles;";

                var cycleCount = Convert.ToInt32(
                    await cycleCommand.ExecuteScalarAsync(
                        cancellationToken));

                using var measurementCommand =
                    connection.CreateCommand();

                measurementCommand.CommandText =
                    "SELECT COUNT(*) FROM measurements;";

                var measurementCount = Convert.ToInt32(
                    await measurementCommand.ExecuteScalarAsync(
                        cancellationToken));

                Assert.Equal(1, cycleCount);
                Assert.Equal(5, measurementCount);
            }
        }
        finally
        {
            // Closed connections may still be retained by SQLite pooling.
            SqliteConnection.ClearAllPools();

            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}