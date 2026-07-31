using MotionControl.Domain;

namespace MotionControl.Domain.Tests;

public sealed class FaultRecoveryPolicyTests
{
    private readonly FaultRecoveryPolicy _policy = new();

    [Theory]
    [InlineData(FaultCode.PartMissing)]
    [InlineData(FaultCode.AirPressureNotReady)]
    [InlineData(FaultCode.InvalidRecipe)]
    [InlineData(FaultCode.OperationCancelled)]
    public void NonPositionFaultsDoNotRequireRehoming(FaultCode faultCode)
    {
        Assert.False(_policy.RequiresRehome(faultCode));
    }

    [Theory]
    [InlineData(FaultCode.EmergencyStopActive)]
    [InlineData(FaultCode.MotionLimitExceeded)]
    [InlineData(FaultCode.HomingFailed)]
    [InlineData(FaultCode.ProbeTimeout)]
    [InlineData(FaultCode.ProbeAlreadyActive)]
    [InlineData(FaultCode.PlcCommunicationLost)]
    public void PositionOrControllerFaultsRequireRehoming(FaultCode faultCode)
    {
        Assert.True(_policy.RequiresRehome(faultCode));
    }

    [Theory]
    [InlineData(FaultCode.MotionControllerUnavailable)]
    [InlineData(FaultCode.UnexpectedSoftwareError)]
    public void ControllerFailuresReturnToOff(FaultCode faultCode)
    {
        Assert.Equal(
            MachineState.Off,
            _policy.GetRecoveryTarget(faultCode));
        Assert.False(_policy.RequiresRehome(faultCode));
    }
}
