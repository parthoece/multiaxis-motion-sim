namespace MotionControl.Simulation;

public sealed class SimulationScenario
{
    public SimulationFault ActiveFault { get; set; } = SimulationFault.None;

    public int Seed { get; init; } = 42;

    public double TimeScale { get; init; } = 0.001;
}
