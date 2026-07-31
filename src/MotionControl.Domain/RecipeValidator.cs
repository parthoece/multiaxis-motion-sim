namespace MotionControl.Domain;

public sealed class RecipeValidator
{
    public IReadOnlyList<string> Validate(InspectionRecipe recipe)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(recipe.RecipeId))
        {
            errors.Add("Recipe ID is required.");
        }

        if (recipe.Version < 1)
        {
            errors.Add("Recipe version must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(recipe.PartNumber))
        {
            errors.Add("Part number is required.");
        }

        if (recipe.SafeZMillimeters is < 0 or > 150)
        {
            errors.Add("Safe Z must be inside the configured Z travel.");
        }

        if (recipe.TravelVelocityMillimetersPerSecond is <= 0 or > 100)
        {
            errors.Add("Travel velocity must be greater than 0 and no more than 100 mm/s.");
        }

        if (recipe.ProbeVelocityMillimetersPerSecond is <= 0 or > 30)
        {
            errors.Add("Probe velocity must be greater than 0 and no more than 30 mm/s.");
        }

        if (recipe.InspectionPoints.Count == 0)
        {
            errors.Add("At least one inspection point is required.");
        }

        foreach (var point in recipe.InspectionPoints)
        {
            if (string.IsNullOrWhiteSpace(point.Name))
            {
                errors.Add("Every inspection point requires a name.");
            }

            if (point.XMillimeters is < 0 or > 500)
            {
                errors.Add($"{point.Name}: X is outside the configured travel.");
            }

            if (point.YMillimeters is < 0 or > 400)
            {
                errors.Add($"{point.Name}: Y is outside the configured travel.");
            }

            if (point.LowerLimitMillimeters > point.UpperLimitMillimeters)
            {
                errors.Add($"{point.Name}: lower limit exceeds upper limit.");
            }

            if (point.NominalZMillimeters < point.LowerLimitMillimeters ||
                point.NominalZMillimeters > point.UpperLimitMillimeters)
            {
                errors.Add($"{point.Name}: nominal Z is outside the tolerance limits.");
            }
        }

        return errors;
    }
}
