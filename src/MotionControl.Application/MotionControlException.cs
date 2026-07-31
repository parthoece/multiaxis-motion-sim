using MotionControl.Domain;

namespace MotionControl.Application;

public sealed class MotionControlException : Exception
{
    public MotionControlException(FaultCode faultCode, string message)
        : base(message)
    {
        FaultCode = faultCode;
    }

    public FaultCode FaultCode { get; }
}
