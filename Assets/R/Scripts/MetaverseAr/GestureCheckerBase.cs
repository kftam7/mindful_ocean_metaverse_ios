using System.Collections.Generic;

public abstract class GestureCheckerBase
{
    private const float ReleaseCheckTime = 0.2f;
    private const float CooldownCheckTime = 3f;
    private float _chargingCheckTime;

    private float _chargedTime;
    private float _unchargedTime = 999;
    private float _cooldownCountdownTime;

    public float ChargedTime => _chargedTime;

    public float ChargingCheckTime => Debugger.Instance.HandTrackingChargingTime > 0
        ? Debugger.Instance.HandTrackingChargingTime
        : _chargingCheckTime;
    
    protected GestureCheckerBase(float chargingCheckTime)
    {
        _chargingCheckTime = chargingCheckTime;
    }

    public void ResetTime()
    {
        _chargedTime = 0;
        _unchargedTime = 999;
        _cooldownCountdownTime = 0;
    }

    public (bool, bool) Check(Dictionary<JointName, JointData> joints, float deltaTime)
    {
        // cool down
        if (_cooldownCountdownTime > 0)
        {
            _cooldownCountdownTime -= deltaTime;
            _chargedTime = 0;
            _unchargedTime = 999;
            return (false, false);
        }

        // find gesture
        if (IsValid(joints) && CheckGesture(joints))
        {
            _unchargedTime = 0;
            _chargedTime += deltaTime;
            return (true, CheckTrigger());
        }

        // no gesture
        _unchargedTime += deltaTime;

        // ignore noise
        if (_unchargedTime < ReleaseCheckTime)
        {
            _chargedTime += deltaTime;
            return (true, CheckTrigger());
        }

        // no gesture for long enough
        _chargedTime = 0;
        return (false, false);
    }

    private bool CheckTrigger()
    {
        bool trigger = false;
        if (_chargedTime >= ChargingCheckTime)
        {
            trigger = true;
            _cooldownCountdownTime = CooldownCheckTime;
        }

        return trigger;
    }

    protected bool IsValid(Dictionary<JointName, JointData> joints)
    {
        int count = 0;
        foreach (JointData jointData in joints.Values)
        {
            if (jointData.confidence >= 0.8f)
            {
                count += 1;
            }
        }

        return count > 12;
    }

    protected abstract bool CheckGesture(Dictionary<JointName, JointData> joints);
    public abstract HandGestureEnum GestureType { get; }
}