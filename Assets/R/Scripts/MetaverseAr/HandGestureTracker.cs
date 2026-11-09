using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class HandGestureTracker : SceneInstance<HandGestureTracker>
{
    [SerializeField] private float _chargingCheckTime = 2f;
    [SerializeField] ARCameraManager _cameraManager;
    [SerializeField] HandDetector _handDetector;

    private List<GestureCheckerBase> _gestureCheckers = new();
    private bool _isCharging;
    private float _chargedTime;
    private GestureCheckerBase _chargingChecker;
    private int _lastLogType; // 0: none, 1: trigger, 2: charging, 3: lost charging 

    public float ChargingCheckTime => Debugger.Instance.HandTrackingChargingTime > 0
        ? Debugger.Instance.HandTrackingChargingTime
        : _chargingCheckTime;

    public event Action<HandGestureEnum, float> GestureChargingEvent;
    public event Action<HandGestureEnum> GestureLostChargingEvent; // lost hand or triggered
    public event Action<HandGestureEnum> GestureTriggerEvent;
    public Dictionary<JointName, JointData> Joints;

    private void Start()
    {
        _gestureCheckers.Add(new HeartGestureChecker(ChargingCheckTime));
        _gestureCheckers.Add(new OkGestureChecker(ChargingCheckTime));
        _gestureCheckers.Add(new BowlGestureChecker(ChargingCheckTime));
        _handDetector.Create();
    }

    private void OnEnable()
    {
        _cameraManager.frameReceived += OnCameraFrameReceived;
        _handDetector.HandDetectedEvent += OnHandDetected;
    }

    private void OnDisable()
    {
        _cameraManager.frameReceived -= OnCameraFrameReceived;
        _handDetector.HandDetectedEvent -= OnHandDetected;
    }

    private void OnDestroy()
    {
        _handDetector.Release();
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
#if UNITY_IOS
        _handDetector.Process();
#endif
    }

    public void ResetTime()
    {
        foreach (GestureCheckerBase checker in _gestureCheckers)
        {
            checker.ResetTime();
        }
    }

    private void OnHandDetected(Dictionary<JointName, JointData> joints)
    {
        Joints = joints;
        if (joints == null)
        {
            if (_isCharging) SetLostEvent();
            return;
        }

        foreach (GestureCheckerBase checker in _gestureCheckers)
        {
            var (isCharging, trigger) = checker.Check(joints, Time.deltaTime);
            if (trigger)
            {
                Debug.Log($"[{GetType().Name}] GestureTriggerEvent {checker.GestureType}");
                _lastLogType = 1;
                GestureTriggerEvent?.Invoke(checker.GestureType);
                return;
            }

            if (isCharging)
            {
                _isCharging = true;
                _chargingChecker = checker;
                if (_lastLogType != 2)
                {
                    Debug.Log($"[{GetType().Name}] GestureChargingEvent {checker.GestureType}");
                    _lastLogType = 2;
                }

                GestureChargingEvent?.Invoke(checker.GestureType, checker.ChargedTime);
                return;
            }
        }

        if (_isCharging)
        {
            SetLostEvent();
        }

        void SetLostEvent()
        {
            _isCharging = false;
            Debug.Log($"[{GetType().Name}] GestureLostChargingEvent {_chargingChecker.GestureType}");
            _lastLogType = 3;
            GestureLostChargingEvent?.Invoke(_chargingChecker.GestureType);
        }
    }
}