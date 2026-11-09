using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MannaSample
{
    /// <summary>
    /// This sample shows how to use Manna for the most common cases:
    /// - Showing a dynamic lighthouse.
    /// - Scanning a dynamic lighthouse and performing an instant calibration into the same session.
    /// * After connecting the QR button on the right part of the screen will show or hide a QR code for a lighthouse that you may scan with another device.
    /// This will trigger the Instant Calibration process that will bring the scanning device in a shared AR session with the host.
    /// * The Leave button will disconnect you from the current Session and connect you to a new one.
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
        /// When pressed will leave the current ConjureKit session.
        /// </summary>
        public Button leaveButton;

        /// <summary>
        /// When connected this toggle will allow you to show/hide the dynamic QR lighthouse
        /// which can be scanned by other devices to join & calibrate into the session of this device.
        /// </summary>
        public Toggle qrCodeToggle;

        /// <summary>
        /// Label that shows currently joined session id.
        /// </summary>
        public Text sessionInfo;

        /// <summary>
        /// Label that shows current ConjureKit state.
        /// </summary>
        public Text conjureKitStateInfo;

        private ConjureKit _conjureKit;
        private Manna _manna;
        private string _lastJoinedSessionId;
        private Texture2D _videoTexture;

        private void Start()
        {
            // Initialize the SDK.
            _conjureKit = new ConjureKit(arCamera.transform, "2e2f5aa8-5869-4c5a-9915-40772173d704", "758323c5-dc7f-42e7-b7b3-1cea5a45131aac95e802-52e7-4993-8ce7-7aeb6f67ea7a");
            
            _conjureKit.OnJoined += session => sessionInfo.text = session.Id;

            _conjureKit.OnLeft += session =>
            {
                if (sessionInfo != null) sessionInfo.text = "";
            };

            _conjureKit.OnStateChanged += state =>
            {
                if (conjureKitStateInfo) conjureKitStateInfo.text = state.ToString();
                ToggleControlsState(state == State.JoinedSession || state == State.Calibrated);
            };

            _manna = new Manna(_conjureKit);

            // Everything is set up so let's connect.
            _conjureKit.Connect();
        }

        private void ToggleControlsState(bool interactable)
        {
            if (leaveButton) leaveButton.interactable = interactable;
            if (qrCodeToggle) qrCodeToggle.interactable = interactable;
        }

        private void Update()
        {
            if (_conjureKit?.GetSession() == null) return;
            FeedMannaWithVideoFrames();
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

        /// <summary>
        /// Toggles showing/hiding of Manna QR lighthouse.
        /// </summary>
        public void ToggleLighthouse() => _manna.SetLighthouseVisible(qrCodeToggle.isOn);

        /// <summary>
        /// Leaves the current session.
        /// </summary>
        public void LeaveSession()
        {
            Debug.Log("Leaving session.");
            _conjureKit.Disconnect();
            _conjureKit.Connect();
        }
    }
}