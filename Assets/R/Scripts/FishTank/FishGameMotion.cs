using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishGameMotion : MonoBehaviour
{
    private const float MaxIdelTime = 1.5f; // max seconds that fish idles when heart is destroyed
    private const float TurnSpeed = 10f;
    private Transform cachedTransform;
    private float idleTime;
    private float moveSpeed => Entity.CurrentSpeed;
    private Transform mouth;
    private float distCenterToMouth;
    private float speedFactor;
    private Quaternion userAxis;
    private Vector3? targetPos;
    private Vector3 destination;
    private Vector3[] pathToTarget;
    private int nextPathPosIndex;
    private Quaternion targetRot;
    private bool arrived;
    private float _minDistance = 0.025f;

    public FishEntity Entity { get; set; }

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        FishMover.OnTapMove += Move;
    }

    private void OnDestroy()
    {
        FishMover.OnTapMove -= Move;
    }

    // private void OnEnable()
    // {
    //     arrived = false;
    //     targetPos = null;
    //     // targetRot = null;
    // }

    private void Update()
    {
        UpdateMovement();
    }

    private void Move(Vector3 pos)
    {
        if (!Entity.Owned) return;

        Logger.Log($"Moving Fish to : {pos}");

        if (Entity.CompFishBody != null && Entity.CompFishBody.Mouth != null)
        {
            Quaternion currentRot = cachedTransform.rotation;
            cachedTransform.rotation = Quaternion.identity;

            mouth = Entity.CompFishBody.Mouth.transform;
            Vector3 fishToMouthVec = mouth.position - cachedTransform.position;
            distCenterToMouth = fishToMouthVec.magnitude;
            userAxis = Quaternion.Inverse(Quaternion.LookRotation(fishToMouthVec.normalized, Vector3.up));
            cachedTransform.rotation = currentRot;
        }

        destination = pos;
        Entity.CompWalker.enabled = false;
        speedFactor = 1f;
        arrived = false;
        targetPos = null;
    }

    private void UpdateMovement()
    {
        if (arrived) return;

        Vector3 fishPos = cachedTransform.position;
        Quaternion fishRot = cachedTransform.rotation;

        if (targetPos == null)
        {
            pathToTarget = AukiSdkWrapper.Instance.CalculatePath(cachedTransform.position, destination);
            nextPathPosIndex = 1;
            Vector3 nextPathPosition = pathToTarget[nextPathPosIndex];

            Vector3 vecHeartToFish = fishPos - nextPathPosition;
            targetPos = nextPathPosition + vecHeartToFish.normalized * distCenterToMouth;

            Quaternion faceRotation = Quaternion.LookRotation(-vecHeartToFish, Vector3.up);
            targetRot = faceRotation * userAxis;
        }

        float dist = Vector3.Distance(fishPos, targetPos.Value);
        bool posArrived = dist <= _minDistance;
        bool dirArrived = Quaternion.Angle(fishRot, targetRot) < 1;
        if (posArrived && dirArrived)
        {
            if (nextPathPosIndex == pathToTarget.Length - 1)
            {
                ReenableCompWalker();
                arrived = true;
                return;
            }
            else
            {
                nextPathPosIndex++;
                Vector3 nextPathPosition = pathToTarget[nextPathPosIndex];

                Vector3 vecHeartToFish = fishPos - nextPathPosition;
                targetPos = nextPathPosition + vecHeartToFish.normalized * distCenterToMouth;

                Quaternion faceRotation = Quaternion.LookRotation(-vecHeartToFish, Vector3.up);
                targetRot = faceRotation * userAxis;
            }
        }

        if (!posArrived)
        {
            float speed = speedFactor * moveSpeed * Mathf.Max(0.2f, Mathf.Min(1, dist / (_minDistance * 5)));
            cachedTransform.position += (targetPos.Value - fishPos).normalized * speed * Time.deltaTime;
        }

        if (!dirArrived)
        {
            cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRot, Mathf.Clamp01(TurnSpeed * Time.deltaTime));
        }
    }

    private void Idle() { }

    private void ReenableCompWalker()
    {
        Entity.CompWalker.CreateArPathFromPosition(cachedTransform.position);
        Entity.CompWalker.enabled = true;
    }
}
