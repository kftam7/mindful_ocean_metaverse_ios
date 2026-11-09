using UnityEngine;
using UnityEngine.EventSystems;
using PaintCraft.Canvas;
using PaintCraft.Tools;
using System.Collections.Generic;
using UnityEngine.Serialization;
using PaintCraft;
using UnityEngine.Rendering;
using System.Collections;
using PaintCraft.Canvas.Configs;

namespace PaintCraft.Controllers{
	[RequireComponent(typeof(Camera))]
    public class ScreenCameraController : InputController {
        public LineConfig LineConfig;
        public Camera Camera { get; private set; }
        public bool ZoomOnMouseScroll = false;
        [FormerlySerializedAs("CameraBounds")]
        public CameraSizeHandler CameraSize;

				protected Vector3 previousPosition;


        CommandBuffer commandBuffer;
        public CommandBuffer CommandBuffer{
            get{
                return commandBuffer;
            }
        }




        void Start(){

            if (Canvas == null){
                Debug.LogError("you have to provide link to the Canvas for this component", gameObject);
            }

            Camera = GetComponent<Camera>();
            if (Camera == null){
                Debug.Log("you have to add camera component to this object", gameObject);
            }

            CameraSize.Init(Camera, Canvas);


            commandBuffer = new CommandBuffer();
            commandBuffer.name = gameObject.name;
            Camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            Canvas.OnPageChange += OnPageChangeHandler;

						CameraSize.SetCameraNewOrthoSize(Camera.orthographicSize * 1.25f );
						//CameraSize.CheckBounds();
        }

        void OnDestroy(){
            Canvas.OnPageChange -= OnPageChangeHandler;
        }

        void OnPageChangeHandler(PageConfig newPage){
            CameraSize.ResetSize();
        }

	    bool IsAnyPointerOverGameObject()
	    {
	        bool result = EventSystem.current.IsPointerOverGameObject();
	        foreach (var touch in Input.touches)
	        {
	            result |= EventSystem.current.IsPointerOverGameObject(touch.fingerId);
	        }

	        return result;
	    }

	    void Update()
	    {
            HandleZoomOnScroll();
            HandleMoveOnScroll();
            if (EventSystem.current != null && IsAnyPointerOverGameObject() == false ){
                if (!HandleTouch()){
                    HandleMouseEvents();
                }
            }
            CameraSize.LateUpdate();
        }

        void OnEnable(){
            StartCoroutine("ClearCommandBuffer");
        }

        void OnDisable(){
            StopCoroutine("ClearCommandBuffer");
        }

        IEnumerator ClearCommandBuffer(){
            while(true){
                yield return new WaitForEndOfFrame();
                if (CommandBuffer != null)
                {
                    CommandBuffer.Clear();
                }
            }
        }



        void HandleZoomOnScroll()
        {
            if (!Input.GetMouseButton(1) && ZoomOnMouseScroll && Camera.pixelRect.Contains(Input.mousePosition))
            {
                float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * 100.0f;
                if (mouseScrollWheel != 0.0f)
                {
                    CameraSize.SetCameraNewOrthoSize(Camera.orthographicSize - mouseScrollWheel );
                    CameraSize.CheckBounds();
                }
            }
        }

        private Vector3 movePoint;

        void HandleMoveOnScroll()
        {
            if (!Camera.pixelRect.Contains(Input.mousePosition)) return;

            if (Input.GetMouseButtonDown(1))
            {
                movePoint = Input.mousePosition;
            }

            if (Input.GetMouseButton(1))
            {
                var deltaPosition = movePoint - Input.mousePosition;
                Camera.transform.position += deltaPosition;
                CameraSize.CheckBounds();
                movePoint = Input.mousePosition;
            }
        }

        /// <summary>
        /// Handles the touch.
        /// </summary>
        /// <returns><c>true</c>, if touch was handled, <c>false</c> otherwise.</returns>
        bool HandleTouch(){
            if (Input.touchCount > 0){
                if (Input.touchCount == 2)
                {
                    isDrawing = false;
                    FingerPinch();
                }
                else
                {
                    foreach (var touch in Input.touches)
                    {
                        Vector3 worldPosition = GetWorldPosition(touch.position);
                        switch (touch.phase)
                        {
                            case TouchPhase.Began:
                                RaycastHit hit;
                                if (LineConfig.Brush.name.Contains("Bucket") && !Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
                                {
                                    Debug.Log("Outside the canvas");
                                }
                                else
                                {
																	Debug.Log("BeginLine HandleTouch");
                                    BeginLine(LineConfig, touch.fingerId, worldPosition, true);
                                }
                                break;
                            case TouchPhase.Canceled:
                            case TouchPhase.Ended:
                                EndLine(touch.fingerId, worldPosition);
                                break;
                            case TouchPhase.Moved:
                            //case TouchPhase.Stationary:
                                ContinueLine(touch.fingerId, worldPosition);
                                break;
                        }
                    }

                }

                return true;
            }
            return false;
        }

        void HandleMouseEvents()
        {
            Vector3 worldPosition = GetWorldPosition(Input.mousePosition);

            if (Input.GetMouseButtonDown(0)){
                RaycastHit hit;
                if (LineConfig.Brush.name.Contains("Bucket") && !Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
                {
                    Debug.Log("Outside the canvas");
                }
                else
                {
									Debug.Log("BeginLine HandleMouseEvents");
                    BeginLine(LineConfig, 0, worldPosition, true);
										previousPosition = worldPosition;
                }
            } else if (Input.GetMouseButton(0)){
								if (previousPosition != worldPosition) {
									ContinueLine(0, worldPosition);
									previousPosition = worldPosition;
								}
            } else if (Input.GetMouseButtonUp(0)){
                EndLine(0, worldPosition);
            }

        }

        public override bool DontAllowInteraction(Vector2 worldPosition)
        {
            bool result = !Camera.pixelRect.Contains(Camera.WorldToScreenPoint( worldPosition));
            result |= IsAnyPointerOverGameObject();
            return result;
        }

        Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            Vector3 vector3ScreenPosition = screenPosition;
            vector3ScreenPosition.z = transform.position.z;
            return Camera.ScreenToWorldPoint(vector3ScreenPosition);
        }

        void FingerPinch()
        {
            // If there are two touches on the device...
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                CameraSize.SetCameraNewOrthoSize(Camera.orthographicSize + deltaMagnitudeDiff);


                Vector2 touchCenterPrevPos = (touchZeroPrevPos + touchOnePrevPos) / 2f;
                Vector2 touchCenterPos = (touchZero.position + touchOne.position) / 2f;
                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float deltaMoveMagnitudeDiff = touchCenterPrevPos.magnitude - touchCenterPos.magnitude;
                Vector2 centerDiff = touchCenterPrevPos - touchCenterPos;

                Debug.Log(deltaMagnitudeDiff);
                Camera.transform.position += new Vector3(centerDiff.x, centerDiff.y);
                CameraSize.CheckBounds();

                //Debug.Log(deltaMagnitudeDiff);
                // ... change the orthographic size based on the change in distance between the touches.
                //camera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Make sure the orthographic size never drops below zero.
                //camera.orthographicSize = Mathf.Max(camera.orthographicSize, 0.1f);
            }
        }

	}
}
