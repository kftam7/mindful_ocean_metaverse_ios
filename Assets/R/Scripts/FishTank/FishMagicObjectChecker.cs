using System.Collections.Generic;
using UnityEngine;

public class FishMagicObjectChecker : MonoBehaviour
{
    private Transform cachedTransform;
    private Transform scaler;
    private bool subCompIsWorking;
    private MonoBehaviour currentCompMagic;

    public FishEntity Entity { get; set; }

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void OnDisable()
    {
        Entity.CompFollowHeart.enabled = false;
        Entity.CompCircleMotion.enabled = false;
        Entity.CompSpiralMotion.enabled = false;
    }

    private void Update()
    {
        if (subCompIsWorking) return;

        if (scaler == null && Entity.CompFishBody != null && Entity.CompFishBody.Scaler != null)
        {
            scaler = Entity.CompFishBody.Scaler;
        }

        if (scaler != null)
        {
            float eulerZ = Mathf.Abs(scaler.localEulerAngles.z);
            if (eulerZ > 0.01f)
            {
                float rotZ = Time.deltaTime * 200;
                scaler.localEulerAngles -= new Vector3(0, 0, rotZ > eulerZ ? eulerZ : rotZ);
            }
        }

        MagicObject target = GetInterestedMagicObject();
        if (target == null) return;
        if (target.Type == MagicObjectTypeEnum.Food)
        {
            Entity.CompFollowHeart.enabled = true;
            Entity.CompFollowHeart.StartPlay(target, OnSubCompStopWorking);
            currentCompMagic = Entity.CompFollowHeart;
        }
        else if (target.Type == MagicObjectTypeEnum.Heart)
        {
            Entity.CompFollowHeart.enabled = true;
            Entity.CompFollowHeart.StartPlay(target, OnSubCompStopWorking);
            currentCompMagic = Entity.CompFollowHeart;
        }
        else if (target.Type == MagicObjectTypeEnum.Torus)
        {
            Entity.CompCircleMotion.enabled = true;
            Entity.CompCircleMotion.StartPlay(target, OnSubCompStopWorking);
            currentCompMagic = Entity.CompCircleMotion;
        }
        else
        {
            Entity.CompSpiralMotion.enabled = true;
            Entity.CompSpiralMotion.StartPlay(target, OnSubCompStopWorking);
            currentCompMagic = Entity.CompSpiralMotion;
        }

        Entity.CompWalker.enabled = false;
        subCompIsWorking = true;
    }

    private List<MagicObject> GetMagicObjects()
    {
        return ARMetaverseManager.Instance.MagicObjects;
    }

    /// <summary>
    /// Get nearest visible heat.
    /// </summary>
    private MagicObject GetInterestedMagicObject()
    {
        MagicObject interestedMagicObject = null;
        Vector3 fishDir = cachedTransform.forward;
        float minSqrDist = 1000000;

        foreach (MagicObject heart in GetMagicObjects())
        {
            if (heart == null) continue;

            var pathToHeart = AukiSdkWrapper.Instance.CalculatePath(cachedTransform.position, heart.CachedTransform.position);
            var nextWaypoint = pathToHeart[1];
            //nextWaypoint.y = heart.CachedTransform.position.y;
            
            Vector3 dirToHeart = nextWaypoint - cachedTransform.position;
            if (Vector3.Dot(fishDir.normalized, dirToHeart.normalized) >= -1f) // all
            {
                float dist = (cachedTransform.position - nextWaypoint).sqrMagnitude;
                if (dist <= minSqrDist)
                {
                    minSqrDist = dist;
                    interestedMagicObject = heart;
                }
            }
        }

        return interestedMagicObject;
    }

    private void OnSubCompStopWorking()
    {
        currentCompMagic.enabled = false;
        subCompIsWorking = false;
        Entity.CompWalker.CreateArPathFromPosition(cachedTransform.position);
        Entity.CompWalker.enabled = true;
    }
}