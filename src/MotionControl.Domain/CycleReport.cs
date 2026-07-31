namespace MotionControl.Domain;

public sealed record CycleReport(
    Guid CycleId,
    string RecipeId,
    int RecipeVersion,
    string PartNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    IReadOnlyList<MeasurementResult> Measurements)
{
    public bool Passed => Measurements.All(measurement => measurement.Passed);

    public TimeSpan Duration => CompletedAt - StartedAt;
}
