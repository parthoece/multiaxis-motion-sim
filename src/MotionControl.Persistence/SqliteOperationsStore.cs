using Microsoft.Data.Sqlite;
using MotionControl.Application;
using MotionControl.Domain;

namespace MotionControl.Persistence;

public sealed class SqliteOperationsStore : IOperationsStore
{
    private readonly string _connectionString;

    public SqliteOperationsStore(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(
            Path.GetDirectoryName(fullPath) ??
            throw new InvalidOperationException("A database directory is required."));

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            ForeignKeys = true,
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS state_transitions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                from_state TEXT NOT NULL,
                to_state TEXT NOT NULL,
                reason TEXT NOT NULL,
                occurred_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS alarms (
                alarm_id TEXT PRIMARY KEY,
                code TEXT NOT NULL,
                message TEXT NOT NULL,
                machine_state TEXT NOT NULL,
                requires_rehome INTEGER NOT NULL,
                raised_at TEXT NOT NULL,
                acknowledged_at TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS cycles (
                cycle_id TEXT PRIMARY KEY,
                recipe_id TEXT NOT NULL,
                recipe_version INTEGER NOT NULL,
                part_number TEXT NOT NULL,
                started_at TEXT NOT NULL,
                completed_at TEXT NOT NULL,
                passed INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS measurements (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                cycle_id TEXT NOT NULL,
                point_name TEXT NOT NULL,
                nominal_mm REAL NOT NULL,
                measured_mm REAL NOT NULL,
                lower_limit_mm REAL NOT NULL,
                upper_limit_mm REAL NOT NULL,
                passed INTEGER NOT NULL,
                FOREIGN KEY(cycle_id) REFERENCES cycles(cycle_id)
            );

            CREATE INDEX IF NOT EXISTS idx_alarms_raised_at
                ON alarms(raised_at);

            CREATE INDEX IF NOT EXISTS idx_cycles_started_at
                ON cycles(started_at);

            CREATE INDEX IF NOT EXISTS idx_measurements_cycle_id
                ON measurements(cycle_id);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveStateTransitionAsync(
        StateTransition transition,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO state_transitions (
                from_state,
                to_state,
                reason,
                occurred_at)
            VALUES (
                $from,
                $to,
                $reason,
                $occurredAt);
            """;

        command.Parameters.AddWithValue("$from", transition.From.ToString());
        command.Parameters.AddWithValue("$to", transition.To.ToString());
        command.Parameters.AddWithValue("$reason", transition.Reason);
        command.Parameters.AddWithValue("$occurredAt", transition.Timestamp.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveAlarmAsync(
        AlarmRecord alarm,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO alarms (
                alarm_id,
                code,
                message,
                machine_state,
                requires_rehome,
                raised_at,
                acknowledged_at)
            VALUES (
                $alarmId,
                $code,
                $message,
                $machineState,
                $requiresRehome,
                $raisedAt,
                $acknowledgedAt)
            ON CONFLICT(alarm_id) DO UPDATE SET
                acknowledged_at = excluded.acknowledged_at;
            """;

        command.Parameters.AddWithValue("$alarmId", alarm.AlarmId.ToString());
        command.Parameters.AddWithValue("$code", alarm.Code.ToString());
        command.Parameters.AddWithValue("$message", alarm.Message);
        command.Parameters.AddWithValue("$machineState", alarm.MachineState.ToString());
        command.Parameters.AddWithValue("$requiresRehome", alarm.RequiresRehome);
        command.Parameters.AddWithValue("$raisedAt", alarm.RaisedAt.ToString("O"));
        command.Parameters.AddWithValue(
            "$acknowledgedAt",
            alarm.AcknowledgedAt?.ToString("O") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveCycleAsync(
        CycleReport report,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        var cycleCommand = connection.CreateCommand();
        cycleCommand.Transaction = (SqliteTransaction)transaction;
        cycleCommand.CommandText =
            """
            INSERT INTO cycles (
                cycle_id,
                recipe_id,
                recipe_version,
                part_number,
                started_at,
                completed_at,
                passed)
            VALUES (
                $cycleId,
                $recipeId,
                $recipeVersion,
                $partNumber,
                $startedAt,
                $completedAt,
                $passed);
            """;

        cycleCommand.Parameters.AddWithValue("$cycleId", report.CycleId.ToString());
        cycleCommand.Parameters.AddWithValue("$recipeId", report.RecipeId);
        cycleCommand.Parameters.AddWithValue("$recipeVersion", report.RecipeVersion);
        cycleCommand.Parameters.AddWithValue("$partNumber", report.PartNumber);
        cycleCommand.Parameters.AddWithValue("$startedAt", report.StartedAt.ToString("O"));
        cycleCommand.Parameters.AddWithValue("$completedAt", report.CompletedAt.ToString("O"));
        cycleCommand.Parameters.AddWithValue("$passed", report.Passed);

        await cycleCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (var measurement in report.Measurements)
        {
            var measurementCommand = connection.CreateCommand();
            measurementCommand.Transaction = (SqliteTransaction)transaction;
            measurementCommand.CommandText =
                """
                INSERT INTO measurements (
                    cycle_id,
                    point_name,
                    nominal_mm,
                    measured_mm,
                    lower_limit_mm,
                    upper_limit_mm,
                    passed)
                VALUES (
                    $cycleId,
                    $pointName,
                    $nominal,
                    $measured,
                    $lower,
                    $upper,
                    $passed);
                """;

            measurementCommand.Parameters.AddWithValue(
                "$cycleId",
                report.CycleId.ToString());
            measurementCommand.Parameters.AddWithValue(
                "$pointName",
                measurement.PointName);
            measurementCommand.Parameters.AddWithValue(
                "$nominal",
                measurement.NominalMillimeters);
            measurementCommand.Parameters.AddWithValue(
                "$measured",
                measurement.MeasuredMillimeters);
            measurementCommand.Parameters.AddWithValue(
                "$lower",
                measurement.LowerLimitMillimeters);
            measurementCommand.Parameters.AddWithValue(
                "$upper",
                measurement.UpperLimitMillimeters);
            measurementCommand.Parameters.AddWithValue(
                "$passed",
                measurement.Passed);

            await measurementCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
