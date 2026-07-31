using MotionControl.Domain;

namespace MotionControl.Domain.Tests;

public sealed class MeasurementResultTests
{
    [Theory]
    [InlineData(9.95)]
    [InlineData(10.00)]
    [InlineData(10.05)]
    public void ToleranceLimitsAreInclusive(double measured)
    {
        var result = new MeasurementResult(
            "P1",
            NominalMillimeters: 10.00,
            MeasuredMillimeters: measured,
            LowerLimitMillimeters: 9.95,
            UpperLimitMillimeters: 10.05);

        Assert.True(result.Passed);
    }

    [Theory]
    [InlineData(9.9499)]
    [InlineData(10.0501)]
    public void MeasurementsOutsideLimitsFail(double measured)
    {
        var result = new MeasurementResult(
            "P1",
            NominalMillimeters: 10.00,
            MeasuredMillimeters: measured,
            LowerLimitMillimeters: 9.95,
            UpperLimitMillimeters: 10.05);

        Assert.False(result.Passed);
    }
}
