using UnityEngine;
using UnityEngine.Events;

namespace BezierSolution
{
    public class BezierWalkerWithSpeed : MonoBehaviour
    {
        public FishEntity Entity { get; set; }

        public enum TravelModeEnum
        {
            Once,
            Loop,
            PingPong
        }

        private Transform cachedTransform;
        public BezierSpline Spline { get; set; }
        public TravelModeEnum TravelMode { get; set; }

        private float Speed => Entity.CurrentSpeed;
        private float progress;

        public float NormalizedT
        {
            set => progress = value;
        }

        [Range(0f, 0.06f)]
        public float relaxationAtEndPoints = 0.01f;

        //public float movementLerpModifier = 10f;
        public float rotationLerpModifier = 10f;

        public bool lookForward = true;

        private bool isGoingForward = true;

        public UnityEvent onPathCompleted = new();
        private bool onPathCompletedCalledAt1;
        private bool onPathCompletedCalledAt0;

        private void Awake()
        {
            cachedTransform = transform;
        }

        private void OnEnable()
        {
            Entity.CompSpeedController.ChangeSpeedRandomly = true;
            Entity.PlayAnimation("swim");
        }

        private void OnDisable()
        {
            Entity.CompSpeedController.ChangeSpeedRandomly = false;
        }

        private void Update()
        {
            float targetSpeed = (isGoingForward) ? Speed : -Speed;

            Vector3 targetPos;
            // Code below uses the obsolete MoveAlongSpline function
            //float absSpeed = Mathf.Abs( speed );
            //if( absSpeed <= 2f )
            //	targetPos = spline.MoveAlongSpline( ref progress, targetSpeed * Time.deltaTime, maximumError: 0f );
            //else if( absSpeed >= 40f )
            //	targetPos = spline.MoveAlongSpline( ref progress, targetSpeed * Time.deltaTime, increasedAccuracy: true );
            //else
            //	targetPos = spline.MoveAlongSpline( ref progress, targetSpeed * Time.deltaTime );

            targetPos = Spline.MoveAlongSpline(ref progress, targetSpeed * Time.deltaTime);

            cachedTransform.position = targetPos;
            //cachedTransform.position = Vector3.Lerp( cachedTransform.position, targetPos, movementLerpModifier * Time.deltaTime );

            bool movingForward = (Speed > 0f) == isGoingForward;

            if (lookForward)
            {
                Quaternion targetRotation;
                if (movingForward)
                    targetRotation = Quaternion.LookRotation(Spline.GetTangent(progress));
                else
                    targetRotation = Quaternion.LookRotation(-Spline.GetTangent(progress));

                cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation, targetRotation, rotationLerpModifier * Time.deltaTime);
            }

            if (movingForward)
            {
                if (progress >= 1f - relaxationAtEndPoints)
                {
                    if (!onPathCompletedCalledAt1)
                    {
                        onPathCompleted.Invoke();
                        onPathCompletedCalledAt1 = true;
                    }

                    if (TravelMode == TravelModeEnum.Once)
                        progress = 1f;
                    else if (TravelMode == TravelModeEnum.Loop)
                        progress -= 1f;
                    else
                    {
                        progress = 2f - progress;
                        isGoingForward = !isGoingForward;
                    }
                }
                else
                {
                    onPathCompletedCalledAt1 = false;
                }
            }
            else
            {
                if (progress <= relaxationAtEndPoints)
                {
                    if (!onPathCompletedCalledAt0)
                    {
                        onPathCompleted.Invoke();
                        onPathCompletedCalledAt0 = true;
                    }

                    if (TravelMode == TravelModeEnum.Once)
                        progress = 0f;
                    else if (TravelMode == TravelModeEnum.Loop)
                        progress += 1f;
                    else
                    {
                        progress = -progress;
                        isGoingForward = !isGoingForward;
                    }
                }
                else
                {
                    onPathCompletedCalledAt0 = false;
                }
            }
        }

        public void CreateArPathFromPosition(Vector3 position)
        {
            TubeFishTank tank = FindObjectOfType<FishTank>() as TubeFishTank;
            Destroy(Spline.gameObject);
            var pathScale = new Vector3(5f, 0, 5f);
            Spline = tank.GetBezierPath(new Vector3(1, 1, 1) + pathScale, new Vector3(0, 1.3f, 0), position);
            NormalizedT = 0;
        }
    }
}