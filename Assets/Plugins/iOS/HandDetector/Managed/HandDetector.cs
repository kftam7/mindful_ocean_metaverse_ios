using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class HandDetector : MonoBehaviour
{
    [SerializeField] private bool _drawLines;
    [SerializeField] private GameObject _lineContainer;
    [SerializeField] private LineRenderer[] _lineRenderers;

    #region Native Bindings

    [DllImport("__Internal")]
    private static extern bool HandDetector_Create(IntPtr session);

    [DllImport("__Internal")]
    private static extern bool HandDetector_Release();

    [DllImport("__Internal")]
    private static extern bool HandDetector_StartDetect();

    #endregion

    #region Fields

    private Dictionary<JointName, JointData> _joints = new();
    private Camera _arCamera;

    #endregion


    #region Callback

    public event Action<Dictionary<JointName, JointData>> HandDetectedEvent;

    #endregion

    #region Methods

    public void Create()
    {
        _arCamera = FindObjectOfType<ARCameraManager>().GetComponent<Camera>();
        foreach (JointName jointName in Enum.GetValues(typeof(JointName)))
        {
            var joint = new JointData
            {
                name = jointName
            };
            _joints.Add(jointName, joint);
        }

        var arSession = FindObjectOfType<ARSession>();
        #if !UNITY_EDITOR
        HandDetector_Create(arSession.subsystem.nativePtr);
        #endif
    }

    public void Release()
    {
        HandDetector_Release();
    }

    public void Process()
    {
        HandDetector_StartDetect();
    }

    /// <summary>
    /// Data format:
    /// ringPIP:0.6931015849113464|0.3677715063095093|0.9790039;littleDIP:0.7821227312088013|0.3248417377471924|0.95654297;
    /// </summary>
    private void OnHandDetectedFromNative(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            _lineContainer.SetActive(false);
            HandDetectedEvent?.Invoke(null);
            return;
        }

        string[] points = data.Split(';');
        foreach (var point in points)
        {
            string[] pointPair = point.Split(':');
            if (pointPair.Length != 2)
            {
                continue;
            }

            string nameKey = pointPair[0];
            if (!Enum.TryParse(nameKey, out JointName jointName))
            {
                continue;
            }

            string[] posVectorString = pointPair[1].Split('|');
            if (posVectorString.Length != 3)
            {
                continue;
            }

            string xString = posVectorString[0];
            string yString = posVectorString[1];

            Vector2 screenPos = new Vector2(Convert.ToSingle(xString, CultureInfo.InvariantCulture),
                Convert.ToSingle(yString, CultureInfo.InvariantCulture));
            _joints[jointName].screenPos = screenPos;
            _joints[jointName].isVisible = true;

            string strConfidence = posVectorString[2];
            float confidence = Convert.ToSingle(strConfidence, CultureInfo.InvariantCulture);
            _joints[jointName].confidence = confidence;
        }

        if (_drawLines) DrawHandFingers();
        _lineContainer.SetActive(_drawLines);
        HandDetectedEvent?.Invoke(_joints);
    }

    private void DrawHandFingers()
    {
        DrawLine(_lineRenderers[0], _joints[JointName.wrist].GetWorldPosition(_arCamera, 1f), _joints[JointName.thumbCMC].GetWorldPosition(_arCamera, 1f), Color.magenta);
        DrawLine(_lineRenderers[1], _joints[JointName.thumbCMC].GetWorldPosition(_arCamera, 1f), _joints[JointName.thumbMP].GetWorldPosition(_arCamera, 1f), Color.blue);
        DrawLine(_lineRenderers[2], _joints[JointName.thumbMP].GetWorldPosition(_arCamera, 1f), _joints[JointName.thumbIP].GetWorldPosition(_arCamera, 1f), Color.yellow);
        DrawLine(_lineRenderers[3], _joints[JointName.thumbIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.thumbTip].GetWorldPosition(_arCamera, 1f), Color.red);
        DrawLine(_lineRenderers[4], _joints[JointName.wrist].GetWorldPosition(_arCamera, 1f), _joints[JointName.indexMCP].GetWorldPosition(_arCamera, 1f), Color.magenta);
        DrawLine(_lineRenderers[5], _joints[JointName.indexMCP].GetWorldPosition(_arCamera, 1f), _joints[JointName.indexPIP].GetWorldPosition(_arCamera, 1f), Color.blue);
        DrawLine(_lineRenderers[6], _joints[JointName.indexPIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.indexDIP].GetWorldPosition(_arCamera, 1f), Color.yellow);
        DrawLine(_lineRenderers[7], _joints[JointName.indexDIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.indexTip].GetWorldPosition(_arCamera, 1f), Color.red);
        DrawLine(_lineRenderers[8], _joints[JointName.wrist].GetWorldPosition(_arCamera, 1f), _joints[JointName.middleMCP].GetWorldPosition(_arCamera, 1f), Color.magenta);
        DrawLine(_lineRenderers[9], _joints[JointName.middleMCP].GetWorldPosition(_arCamera, 1f), _joints[JointName.middlePIP].GetWorldPosition(_arCamera, 1f), Color.blue);
        DrawLine(_lineRenderers[10], _joints[JointName.middlePIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.middleDIP].GetWorldPosition(_arCamera, 1f), Color.yellow);
        DrawLine(_lineRenderers[11], _joints[JointName.middleDIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.middleTip].GetWorldPosition(_arCamera, 1f), Color.red);
        DrawLine(_lineRenderers[12], _joints[JointName.wrist].GetWorldPosition(_arCamera, 1f), _joints[JointName.ringMCP].GetWorldPosition(_arCamera, 1f), Color.magenta);
        DrawLine(_lineRenderers[13], _joints[JointName.ringMCP].GetWorldPosition(_arCamera, 1f), _joints[JointName.ringPIP].GetWorldPosition(_arCamera, 1f), Color.blue);
        DrawLine(_lineRenderers[14], _joints[JointName.ringPIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.ringDIP].GetWorldPosition(_arCamera, 1f), Color.yellow);
        DrawLine(_lineRenderers[15], _joints[JointName.ringDIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.ringTip].GetWorldPosition(_arCamera, 1f), Color.red);
        DrawLine(_lineRenderers[16], _joints[JointName.wrist].GetWorldPosition(_arCamera, 1f), _joints[JointName.littleMCP].GetWorldPosition(_arCamera, 1f), Color.magenta);
        DrawLine(_lineRenderers[17], _joints[JointName.littleMCP].GetWorldPosition(_arCamera, 1f), _joints[JointName.littlePIP].GetWorldPosition(_arCamera, 1f), Color.blue);
        DrawLine(_lineRenderers[18], _joints[JointName.littlePIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.littleDIP].GetWorldPosition(_arCamera, 1f), Color.yellow);
        DrawLine(_lineRenderers[19], _joints[JointName.littleDIP].GetWorldPosition(_arCamera, 1f), _joints[JointName.littleTip].GetWorldPosition(_arCamera, 1f), Color.red);
    }

    private void DrawLine(LineRenderer renderer, Vector3 start, Vector3 end, Color color)
    {
        renderer.startColor = color;
        renderer.endColor = color;

        renderer.startWidth = 0.002f;
        renderer.endWidth = 0.002f;

        renderer.SetPosition(0, start);
        renderer.SetPosition(1, end);
    }

    #endregion
}