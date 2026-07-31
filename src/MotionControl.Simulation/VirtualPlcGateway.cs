using MotionControl.Application;
using MotionControl.Domain;

namespace MotionControl.Simulation;

public sealed class VirtualPlcGateway : IPlcGateway
{
    private readonly SimulationScenario _scenario;

    public VirtualPlcGateway(SimulationScenario scenario)
    {
        _scenario = scenario;
    }

    public PlcOutputs LastOutputs { get; private set; } = PlcOutputs.Off;

    public Task<SafetyInputs> ReadInputsAsync(CancellationToken cancellationToken)
    {
        if (_scenario.ActiveFault == SimulationFault.PlcCommunicationLost)
        {
            throw new MotionControlException(
                FaultCode.PlcCommunicationLost,
                "The virtual PLC stopped responding.");
        }

        var inputs = SafetyInputs.Ready with
        {
            EmergencyStopReset =
                _scenario.ActiveFault != SimulationFault.EmergencyStopActive,
            DoorClosed =
                _scenario.ActiveFault != SimulationFault.DoorOpen,
            PartPresent =
                _scenario.ActiveFault != SimulationFault.PartMissing,
            AirPressureReady =
                _scenario.ActiveFault != SimulationFault.AirPressureNotReady,
        };

        return Task.FromResult(inputs);
    }

    public Task WriteOutputsAsync(
        PlcOutputs outputs,
        CancellationToken cancellationToken)
    {
        if (_scenario.ActiveFault == SimulationFault.PlcCommunicationLost)
        {
            throw new MotionControlException(
                FaultCode.PlcCommunicationLost,
                "The virtual PLC stopped responding.");
        }

        LastOutputs = outputs;
        return Task.CompletedTask;
    }
}
