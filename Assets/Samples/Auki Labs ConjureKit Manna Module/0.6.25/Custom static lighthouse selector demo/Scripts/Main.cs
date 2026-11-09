using System;
using System.Linq;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MannaSampleCustomStaticLighthouseSelectorDemo
{
    /// <summary>
    /// This sample shows how to implement a custom static lighthouse selector for Manna.
    /// Before use a static lighthouse needs to be created in the Auki Console and placed with Auki Lightkeeper.
    /// Static lighthouses can be included in many domains so we've provided the ability for the app
    /// to choose which particular domain lighthouse pose will be used for calibration.
    /// When a static lighthouse is scanned the SelectLatestStaticLighthousePose method will be invoked
    /// and it will choose the latest placement and will calibrate from it. The statusLabel will be updated when the calibration process completes.
    /// </summary>
    public class Main : MonoBehaviour
    {
        /// <summary>
        /// AR Camera Manager necessary to supply Manna with camera feed frames.
        /// </summary>
        public ARCameraManager arCameraManager;
        
        /// <summary>
        /// AR Camera necessary to supply Manna with camera transformation matrices and for ConjureKit's constructor.
        /// </summary>
        public Camera arCamera;
        
        /// <summary>
        /// Label that will indicate when a calibration from a static lighthouse has been achieved.
        /// </summary>
        public Text statusLabel;
        
        private ConjureKit _conjureKit;
        private Manna _manna;
        private Texture2D _videoTexture;
        private bool _waitingForStaticLighthouseCalibration;
        
        void Start()
        {
            // Initialize the SDK.
            _conjureKit = new ConjureKit(arCamera.transform, "2e2f5aa8-5869-4c5a-9915-40772173d704", "758323c5-dc7f-42e7-b7b3-1cea5a45131aac95e802-52e7-4993-8ce7-7aeb6f67ea7a");

            // Initialize Manna and provide a custom static lighthouse pose selector.
            _manna = new Manna(_conjureKit);
            _manna.SetStaticLighthousePoseSelector(SelectLatestStaticLighthousePose);
            
            // Subscribe to Manna calibration success handler and wait for static lighthouse calibration.
            _manna.OnCalibrationSuccess += _ =>
            {
                if (!_waitingForStaticLighthouseCalibration || _conjureKit.GetState() != State.Calibrated) return;
                _waitingForStaticLighthouseCalibration = false;
                statusLabel.text = "Calibrated from a static lighthouse";
            };

            // Subscribe to calibration failure handler in case something goes wrong.
            _manna.OnCalibrationFailed += data =>
            {
                statusLabel.text = "Calibration failed, check console log";
                Debug.Log($"Static lighthouse calibration failed: {data.Reason}");
            };

            // Everything is set up so let's connect.
            _conjureKit.Connect();
        }
        
        private void Update()
        {
            if (_conjureKit?.GetSession() == null) return;
            FeedMannaWithVideoFrames();
        }
        
        /// <summary>
        /// This basic implementation of the static lighthouse pose selector will choose the pose from the latest placement.
        /// </summary>
        /// <param name="lighthouseData">Array of LighthousePoses</param>
        /// <param name="onPoseSelected">Call with chosen LighthousePose</param>
        private void SelectLatestStaticLighthousePose(StaticLighthouseData lighthouseData, Action<LighthousePose> onPoseSelected)
        {
            _waitingForStaticLighthouseCalibration = true;
            onPoseSelected(lighthouseData.poses.OrderBy(p => p.placedAt).Last());
        }
        
        /// <summary>
        /// Manna needs to be supplied with camera feed frames so it can detect QR codes and perform Instant Calibration.
        /// For this particular Sample we'll be using AR Foundations AR Camera Manager to retrieve the images.
        /// </summary>
        private void FeedMannaWithVideoFrames()
        {
            var imageAcquired = arCameraManager.TryAcquireLatestCpuImage(out var cpuImage);
            if (!imageAcquired)
            {
                Debug.Log("Couldn't acquire CPU image");
                return;
            }

            if (_videoTexture == null) _videoTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.R8, false);

            var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.R8);
            cpuImage.ConvertAsync(
                conversionParams, 
                (_, _, buffer) =>
                {
                    _videoTexture.SetPixelData(buffer, 0, 0);
                    _videoTexture.Apply();
                    cpuImage.Dispose();
                    
                    _manna.ProcessVideoFrameTexture(
                        _videoTexture,
                        arCamera.projectionMatrix,
                        arCamera.worldToCameraMatrix
                    );
                }
            );
        }
    }
}