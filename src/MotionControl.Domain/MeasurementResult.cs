namespace MotionControl.Domain;

public sealed record MeasurementResult(
    string PointName,
    double NominalMillimeters,
    double MeasuredMillimeters,
    double LowerLimitMillimeters,
    double UpperLimitMillimeters)
{
    public double ErrorMillimeters => MeasuredMillimeters - NominalMillimeters;

    public bool Passed =>
        MeasuredMillimeters >= LowerLimitMillimeters &&
        MeasuredMillimeters <= UpperLimitMillimeters;
}
