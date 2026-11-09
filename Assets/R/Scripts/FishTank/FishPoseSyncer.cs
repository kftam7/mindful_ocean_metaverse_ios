using Auki.ConjureKit;
using Auki.ConjureKit.Vikja;
using UnityEngine;

public class FishPoseSyncer : MonoBehaviour
{
    public FishEntity Entity { get; set; }
    private Transform scaler;
    private Transform cachedTransform;
    private Vector3 lastRemotePosition;
    private Quaternion lastRemoteRotation;
    private float checkingTimeStep = 2f;
    private float lastTime;
    private Vector3 remoteNextPosition;
    private Quaternion remoteNextRotation;
    private float moveSpeed;
    private bool isScrolling;
    private float scrollSpeed;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        lastRemotePosition = cachedTransform.position;
        lastTime = Time.realtimeSinceStartup;
        if (Entity.CompFishBody != null && Entity.CompFishBody.Scaler != null)
        {
            scaler = Entity.CompFishBody.Scaler;
        }
    }

    private void OnEnable()
    {
        AukiSdkWrapper.Instance.EntityUpdatePoseEvent += OnEntityUpdate;
        AukiSdkWrapper.Instance.EntityActionEvent += OnEntityAction;
    }

    private void OnDisable()
    {
        if (AukiSdkWrapper.Instance != null)
        {
            AukiSdkWrapper.Instance.EntityUpdatePoseEvent -= OnEntityUpdate;
            AukiSdkWrapper.Instance.EntityActionEvent -= OnEntityAction;
        }
    }

    private void LateUpdate()
    {
        if (!AukiSdkWrapper.Instance.HasJoined || Entity == null || Entity.AukiEntity == null) return;

        if (Entity.Owned)
        {
            AukiSdkWrapper.Instance.UpdateEntityPose(Entity.AukiEntity.Id, new Pose(cachedTransform.position, cachedTransform.rotation));
        }
        else
        {
            if (Entity.CompWalker.enabled) Entity.CompWalker.enabled = false;
            if (Entity.CompMagicObjectChecker.enabled) Entity.CompMagicObjectChecker.enabled = false;
            Vector3 position = cachedTransform.position + (remoteNextPosition - cachedTransform.position).normalized * (moveSpeed * Time.deltaTime);
            cachedTransform.SetPositionAndRotation(position, remoteNextRotation);

            if (scaler != null)
            {
                if (isScrolling)
                {
                    scaler.localEulerAngles -= new Vector3(0, 0, Time.deltaTime * scrollSpeed);
                }
                else
                {
                    float eulerZ = Mathf.Abs(scaler.localEulerAngles.z);
                    if (eulerZ > 0.01f)
                    {
                        float rotZ = Time.deltaTime * 200;
                        scaler.localEulerAngles -= new Vector3(0, 0, rotZ > eulerZ ? eulerZ : rotZ);
                    }
                }
            }
        }
    }

    private void OnEntityUpdate(Entity entity)
    {
        if (Entity.AukiEntity == null || entity.Id != Entity.AukiEntity.Id) return;

        var pose = AukiSdkWrapper.Instance.GetEntityPose(Entity.AukiEntity);
        Vector3 remotePosition = pose.position;
        Vector3 remoteDeltaVec = remotePosition - lastRemotePosition;
        lastRemotePosition = remotePosition;
        float remoteSpeed = remoteDeltaVec.magnitude / (Time.realtimeSinceStartup - lastTime);
        remoteNextPosition = remotePosition + remoteDeltaVec.normalized * (remoteSpeed * checkingTimeStep);
        moveSpeed = (remoteNextPosition - cachedTransform.position).magnitude / checkingTimeStep;

        remoteNextRotation = pose.rotation;
        lastRemoteRotation = pose.rotation;

        lastTime = Time.realtimeSinceStartup;
    }

    private void OnEntityAction(EntityAction action)
    {
        if (Entity.AukiEntity == null) return;
        if (action.Name == EntityActionNameEnum.StartScroll && action.EntityId == Entity.AukiEntity.Id)
        {
            isScrolling = true;
            scrollSpeed = DataConverter.BytesToType<float>(action.Data);
        }

        if (action.Name == EntityActionNameEnum.StopScroll && action.EntityId == Entity.AukiEntity.Id)
        {
            isScrolling = false;
        }
    }
}