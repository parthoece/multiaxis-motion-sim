namespace MotionControl.Domain;

public sealed record InspectionRecipe(
    string RecipeId,
    int Version,
    string PartNumber,
    double SafeZMillimeters,
    double TravelVelocityMillimetersPerSecond,
    double ProbeVelocityMillimetersPerSecond,
    IReadOnlyList<InspectionPoint> InspectionPoints)
{
    public static InspectionRecipe Demo => new(
        RecipeId: "PLATE-160X80",
        Version: 1,
        PartNumber: "DEMO-PLATE-01",
        SafeZMillimeters: 20,
        TravelVelocityMillimetersPerSecond: 80,
        ProbeVelocityMillimetersPerSecond: 5,
        InspectionPoints:
        [
            new("P1", 20, 20, 10.00, 9.95, 10.05),
            new("P2", 180, 20, 10.00, 9.95, 10.05),
            new("P3", 20, 100, 10.00, 9.95, 10.05),
            new("P4", 180, 100, 10.00, 9.95, 10.05),
            new("P5", 100, 60, 10.00, 9.95, 10.05),
        ]);
}
