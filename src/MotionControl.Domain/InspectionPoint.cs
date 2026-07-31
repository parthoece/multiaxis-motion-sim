namespace MotionControl.Domain;

public sealed record InspectionPoint(
    string Name,
    double XMillimeters,
    double YMillimeters,
    double NominalZMillimeters,
    double LowerLimitMillimeters,
    double UpperLimitMillimeters);
