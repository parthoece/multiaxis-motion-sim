namespace MotionControl.Hmi.Wpf;

public sealed record MeasurementRow(
    string Point,
    double NominalMillimeters,
    double MeasuredMillimeters,
    double ErrorMillimeters,
    bool Passed)
{
    public string Result => Passed ? "PASS" : "FAIL";
}
