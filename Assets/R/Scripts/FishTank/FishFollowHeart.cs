using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishFollowHeart : MonoBehaviour
{
    private const float MaxIdelTime = 1.5f; // max seconds that fish idles when heart is destroyed
    private const float TurnSpeed = 10f;
    private Transform cachedTransform;
    private float idleTime;
    private float moveSpeed => Entity.CurrentSpeed;
    private Transform mouth;
    private float distCenterToMouth;
    private bool compIsWorking;
    private Action endCallback;
    private MagicObject magicObject;
    private float speedFactor;
    private Quaternion userAxis;
    private Vector3? targetPos;
    private Vector3[] pathToTarget;
    private int nextPathPosIndex;
    private Quaternion? targetRot;
    private bool arrived;

    public FishEntity Entity { get; set; }
    public float MinDistance { get; set; }

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void OnEnable()
    {
        arrived = false;
        targetPos = null;
        targetRot = null;
    }

    private void Update()
    {
        if (magicObject == null)
        {
            if (idleTime <= 0)
            {
                idleTime = Random.Range(1, MaxIdelTime);
                Idle();
            }
            else
            {
                idleTime -= Time.deltaTime;
                if (idleTime <= 0) endCallback?.Invoke(); // disable from external
                else Idle();
            }
        }
        else
        {
            FollowHeart();
        }
    }

    public void StartPlay(MagicObject magicObject, Action endCallback)
    {
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

        this.magicObject = magicObject;
        this.endCallback = endCallback;
        var magicObjectHeart = magicObject as MagicObjectHeart;
        speedFactor = magicObjectHeart.SpeedFactor;
    }

    private void FollowHeart()
    {
        if (arrived) return;
        Vector3 fishPos = cachedTransform.position;
        Quaternion fishRot = cachedTransform.rotation;
        if (targetPos == null)
        {
            Vector3 heartPos = magicObject.CachedTransform.position;
            pathToTarget = AukiSdkWrapper.Instance.CalculatePath(cachedTransform.position, heartPos);
            nextPathPosIndex = 1;
            Vector3 nextPathPosition = pathToTarget[nextPathPosIndex];

            Vector3 vecHeartToFish = fishPos - nextPathPosition;
            targetPos = nextPathPosition + vecHeartToFish.normalized * distCenterToMouth;

            Quaternion faceRotation = Quaternion.LookRotation(-vecHeartToFish, Vector3.up);
            targetRot = faceRotation * userAxis;
        }

        float dist = Vector3.Distance(fishPos, targetPos.Value);
        bool posArrived = dist <= MinDistance;
        bool dirArrived = Quaternion.Angle(fishRot, targetRot.Value) < 1;
        if (posArrived && dirArrived)
        {
            if (nextPathPosIndex == pathToTarget.Length - 1)
            {
                ((MagicObjectHeart)magicObject).BeTouched();
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
            float speed = speedFactor * moveSpeed * Mathf.Max(0.2f, Mathf.Min(1, dist / (MinDistance * 5)));
            cachedTransform.position += (targetPos.Value - fishPos).normalized * speed * Time.deltaTime;
        }

        if (!dirArrived)
        {
            cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRot.Value, Mathf.Clamp01(TurnSpeed * Time.deltaTime));
        }
    }

    private void Idle() { }
}