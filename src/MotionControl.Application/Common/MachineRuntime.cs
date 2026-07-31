using MotionControl.Domain;

namespace MotionControl.Application;

internal sealed class MachineRuntime
{
    private readonly object _sync = new();
    private readonly MachineStateMachine _stateMachine = new();

    private AlarmRecord? _activeAlarm;
    private SafetyInputs _lastSafetyInputs = SafetyInputs.Ready;

    public MachineState CurrentState
    {
        get
        {
            lock (_sync)
            {
                return _stateMachine.Current;
            }
        }
    }

    public AlarmRecord? ActiveAlarm
    {
        get
        {
            lock (_sync)
            {
                return _activeAlarm;
            }
        }
    }

    public SafetyInputs LastSafetyInputs
    {
        get
        {
            lock (_sync)
            {
                return _lastSafetyInputs;
            }
        }
    }

    public StateTransition TransitionTo(
        MachineState target,
        string reason,
        DateTimeOffset timestamp)
    {
        lock (_sync)
        {
            return _stateMachine.TransitionTo(target, reason, timestamp);
        }
    }

    public StateTransition EnterFault(
        string reason,
        DateTimeOffset timestamp)
    {
        lock (_sync)
        {
            return _stateMachine.EnterFault(reason, timestamp);
        }
    }

    public void SetActiveAlarm(AlarmRecord alarm)
    {
        lock (_sync)
        {
            _activeAlarm = alarm;
        }
    }

    public AlarmRecord AcknowledgeActiveAlarm(DateTimeOffset timestamp)
    {
        lock (_sync)
        {
            if (_activeAlarm is null)
            {
                throw new DomainException(
                    "An active alarm is required before acknowledgement.");
            }

            _activeAlarm = _activeAlarm.Acknowledge(timestamp);
            return _activeAlarm;
        }
    }

    public void ClearActiveAlarm()
    {
        lock (_sync)
        {
            _activeAlarm = null;
        }
    }

    public void SetSafetyInputs(SafetyInputs safetyInputs)
    {
        lock (_sync)
        {
            _lastSafetyInputs = safetyInputs;
        }
    }
}
