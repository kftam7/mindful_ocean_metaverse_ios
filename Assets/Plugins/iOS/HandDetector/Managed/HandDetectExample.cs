using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class HandDetectExample : MonoBehaviour
{
    [SerializeField] ARCameraManager _cameraManager;
    [SerializeField] HandDetector _handDetector;

    private void Start()
    {
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

    private void OnHandDetected(Dictionary<JointName, JointData> joints)
    {
        // do something
    }
}