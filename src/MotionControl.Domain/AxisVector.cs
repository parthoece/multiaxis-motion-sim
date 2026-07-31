namespace MotionControl.Domain;

public readonly record struct AxisVector(double X, double Y, double Z)
{
    public static AxisVector Origin => new(0, 0, 0);

    public double Get(AxisId axis) => axis switch
    {
        AxisId.X => X,
        AxisId.Y => Y,
        AxisId.Z => Z,
        _ => throw new NotSupportedException($"Axis {axis} is not supported by the XYZ profile."),
    };

    public AxisVector With(AxisId axis, double value) => axis switch
    {
        AxisId.X => this with { X = value },
        AxisId.Y => this with { Y = value },
        AxisId.Z => this with { Z = value },
        _ => throw new NotSupportedException($"Axis {axis} is not supported by the XYZ profile."),
    };
}
