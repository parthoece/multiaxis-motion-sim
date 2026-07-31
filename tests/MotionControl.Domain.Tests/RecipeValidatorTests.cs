using MotionControl.Domain;

namespace MotionControl.Domain.Tests;

public sealed class RecipeValidatorTests
{
    [Fact]
    public void DemoRecipeIsValid()
    {
        var errors = new RecipeValidator().Validate(InspectionRecipe.Demo);

        Assert.Empty(errors);
    }

    [Fact]
    public void OutOfRangePointIsRejected()
    {
        var invalid = InspectionRecipe.Demo with
        {
            InspectionPoints =
            [
                new InspectionPoint("BAD", 900, 20, 10, 9.9, 10.1),
            ],
        };

        var errors = new RecipeValidator().Validate(invalid);

        Assert.Contains(
            errors,
            error => error.Contains("X is outside", StringComparison.Ordinal));
    }
}
