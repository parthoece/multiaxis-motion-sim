using MotionControl.Domain;

namespace MotionControl.GrblHal;

public sealed record GrblHalOptions
{
    public string Host { get; init; } = "127.0.0.1";

    public int Port { get; init; } = 23000;

    public TimeSpan CommandTimeout { get; init; } =
        TimeSpan.FromSeconds(10);

    public TimeSpan MotionTimeout { get; init; } =
        TimeSpan.FromSeconds(90);

    public TimeSpan StatusPollInterval { get; init; } =
        TimeSpan.FromMilliseconds(75);

    // Enables a complete hardware-free demonstration. grblHAL still executes
    // the G-code motion, while home switches and probe contact are modeled here.
    public bool SoftwareOnlyInputs { get; init; } = true;

    public AxisVector SimulatedHomePosition { get; init; } =
        new(0, 0, 150);

    public double SimulatedProbeSurfaceZ { get; init; } = 10.0;

    public bool UnlockAlarmOnInitialize { get; init; } = true;
}
