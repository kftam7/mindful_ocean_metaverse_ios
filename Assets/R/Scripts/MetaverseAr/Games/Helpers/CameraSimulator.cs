using System.Collections;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARFoundation;

namespace McKenna.Debugs
{
    public class CameraSimulator : MonoBehaviour
    {
        public float movementSpeed = 5f;
        public float mouseSensitivity = 2f;

        [SerializeField] private Transform camera;

        private bool _disableControl;

        private void Start()
        {
#if UNITY_EDITOR
            if (camera.TryGetComponent<TrackedPoseDriver>(out var poseDriver))
            {
                poseDriver.enabled = false;
            }
#else
            Destroy(this);
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                _disableControl = !_disableControl;
            }
            
            if (_disableControl) return;
            
            // Handle player movement
            MovePlayer();

            // Handle player rotation based on mouse input
            RotatePlayer();
        }

        private void MovePlayer()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = camera.forward * vertical + camera.right * horizontal;
            camera.position += movement * (Time.deltaTime * movementSpeed);
        }

        private void RotatePlayer()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the player around the Y-axis (left and right)
            camera.Rotate(Vector3.up * mouseX);

            // Rotate the camera around the X-axis (up and down)
            float newRotationX = camera.eulerAngles.x - mouseY;

            // Clamp the vertical rotation to prevent flipping
            newRotationX = Mathf.Clamp(newRotationX, 0f, 90f);

            camera.rotation = Quaternion.Euler(newRotationX, camera.eulerAngles.y, 0f);
        }
    }
}
