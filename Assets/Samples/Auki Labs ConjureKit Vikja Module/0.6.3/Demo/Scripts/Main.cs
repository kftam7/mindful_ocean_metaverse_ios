using System.Text;
using Auki.ConjureKit;
using Auki.ConjureKit.Vikja;
using Auki.Util;
using UnityEngine;
using UnityEngine.UI;

namespace VikjaSample
{
    /// <summary>
    /// This sample shows a common Vikja use case - broadcasting a custom action on a spawned entity.
    /// * On start the demo will join a new session.
    /// * Labels will indicate joined session id & the ConjureKit state.
    /// * The InputField in the top right can be used to join another existing session by its id.
    /// * The Spawn button at the bottom of the screen will, when in a session with another participant(s),
    /// instantiate an entity and request a custom Vikja action on it.
    /// * The Leave button will disconnect you from the current session and join new one.
    /// </summary>
    public class Main : MonoBehaviour
    {
        /// <summary>
        /// Transform of the camera GameObject. Required by ConjureKit.
        /// </summary>
        public Transform cameraTransform;
        
        /// <summary>
        /// When connected pressing this button will instantiate an entity 50 cm in front of the device.
        /// </summary>
        public Button spawnButton;
       
        /// <summary>
        /// When pressed will leave the current ConjureKit session.
        /// </summary>
        public Button leaveButton;

        /// <summary>
        /// Label that shows currently joined session id.
        /// </summary>
        public Text sessionInfo;
        
        /// <summary>
        /// Label that shows current ConjureKit state.
        /// </summary>
        public Text conjureKitStateInfo;
        
        /// <summary>
        /// Input field that allows you to specify session to join by id.
        /// If the session does not exist the participant will be redirected to a new one.
        /// </summary>
        public InputField customSessionInputField;

        private IConjureKit _conjureKit;
        private Vikja _vikja;
        private string _lastJoinedSessionId;

        private void Start()
        {
            // Initialize the SDK.
            _conjureKit = new ConjureKit(cameraTransform, "2e2f5aa8-5869-4c5a-9915-40772173d704", "insert_app_secret_here");

            _conjureKit.OnJoined += session =>
            {
                sessionInfo.text = session.Id;
                _lastJoinedSessionId = session.Id;
            };

            _conjureKit.OnLeft += session =>
            {
                if (sessionInfo != null) sessionInfo.text = "";
            };

            _conjureKit.OnStateChanged += state =>
            {
                if (conjureKitStateInfo) conjureKitStateInfo.text = state.ToString();
                ToggleControlsState(state == State.JoinedSession || state == State.Calibrated);
            };

            _vikja = new Vikja(_conjureKit);
            
            // Subscribe to Vikja entity actions.
            _vikja.OnEntityAction += action => Debug.Log($"Received action {action.Name} for entity {action.EntityId}.");

            // Everything is set up so let's connect.
            _conjureKit.Connect();
        }
        
        /// <summary>
        /// Instantiates an entity in front of the phone.
        /// A Vikja action named "VIKJA_TEST_ACTION" containing a string message will be invoked on it and broadcast to the session participants.
        /// </summary>
        public void Spawn()
        {
            var pose = new Pose(cameraTransform.position + cameraTransform.forward * 0.5f, Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0));

            _conjureKit.GetSession()?.AddEntity(pose, false, entity =>
            {
                _vikja.RequestAction(entity.Id, "VIKJA_TEST_ACTION", Encoding.ASCII.GetBytes("VIKJA_TEST_ACTION"),
                    action => { AukiDebug.LogInfo($"Received request action success with {action.Name}"); },
                    s => AukiDebug.LogInfo($"Received request action error: {s}"));
            }, s => AukiDebug.LogDebug($"Received entity add error error: {s}"));
        }

        /// <summary>
        /// Leaves the current session.
        /// </summary>
        public void LeaveSession()
        {
            Debug.Log("Leaving session.");
            _conjureKit.Disconnect();
            _conjureKit.Connect();
        }

        private void ToggleControlsState(bool interactable)
        {
            spawnButton.interactable = interactable;
            leaveButton.interactable = interactable;
        }

        /// <summary>
        /// Tries to join a session by the id in the custom session InputField in the Settings panel.
        /// </summary>
        public void JoinCustomSessionButtonPressed()
        {
            var targetSessionId = customSessionInputField.text;
            if (string.IsNullOrEmpty(targetSessionId) || targetSessionId == _lastJoinedSessionId) return;
            Debug.Log($"Joining custom session: {targetSessionId}");
            _conjureKit.Disconnect();
            _conjureKit.Connect(targetSessionId, _ =>
            {
                Debug.LogWarning($"Failed to join session {targetSessionId}, connecting to a new session instead.");
                _conjureKit.Connect();
            });
        }
    }
}