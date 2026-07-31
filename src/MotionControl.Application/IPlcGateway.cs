using MotionControl.Domain;

namespace MotionControl.Application;

public interface IPlcGateway
{
    Task<SafetyInputs> ReadInputsAsync(CancellationToken cancellationToken);

    Task WriteOutputsAsync(
        PlcOutputs outputs,
        CancellationToken cancellationToken);
}
