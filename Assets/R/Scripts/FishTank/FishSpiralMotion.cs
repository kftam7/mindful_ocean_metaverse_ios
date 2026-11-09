using System;
using UnityEngine;

public class FishSpiralMotion : MonoBehaviour
{
    private float radius;
    private float height;
    private float angle;
    private bool isPlaying;
    private Transform cachedTransform;
    private float moveSpeed => Entity.CurrentSpeed;
    private float speedFactor;
    private float ySpeedFactor;
    private int phase;
    private bool triggered;
    private Transform magicObjectTransform;
    private MagicObject magicObject;
    private Action endCallback;
    public FishEntity Entity { get; set; }

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Update()
    {
        if (!isPlaying) return;
        if (magicObject == null)
        {
            isPlaying = false;
            endCallback?.Invoke(); // will disable component from external
        }
        else
        {
            SpiralMove();
        }
    }

    public void StartPlay(MagicObject magicObject, Action endCallback)
    {
        this.endCallback = endCallback;
        this.magicObject = magicObject;
        magicObjectTransform = magicObject.transform;
        var magicObjectBowl = magicObject as MagicObjectBowl;
        radius = magicObjectBowl.Radius * Mathf.Pow(Entity.CharacterData.RealSizeRef, 0.2f);
        float sqrtRealSizeRef = Mathf.Sqrt(Entity.CharacterData.RealSizeRef);
        speedFactor = magicObjectBowl.SpeedFactor * sqrtRealSizeRef;
        ySpeedFactor = magicObjectBowl.YSpeedFactor * Entity.CharacterData.RealSizeRef;
        isPlaying = true;
        phase = 0;
        triggered = false;
    }

    private void SpiralMove()
    {
        float deltaTime = Time.deltaTime;
        var fishPosition = cachedTransform.position;
        var objectPosition = magicObjectTransform.position;
        Vector3 hVecObjectToFish = fishPosition - objectPosition;
        hVecObjectToFish.y = 0;
        float hDist = hVecObjectToFish.magnitude;
        hDist = Mathf.Min(hDist, 2 * radius);
        float turnAngle = 90f * hDist / radius;
        Vector3 hMoveDir = RotateVectorAroundUp(hVecObjectToFish, turnAngle).normalized;
        Vector3 newPos = fishPosition + hMoveDir * moveSpeed * speedFactor * deltaTime;
        float vMoveDist = 0;
        if (phase == 0) // go to start point
        {
            float vDist = objectPosition.y - fishPosition.y;
            vMoveDist = Mathf.Sign(vDist) * speedFactor * moveSpeed * deltaTime;
            if (Mathf.Abs(vMoveDist) > Mathf.Abs(vDist))
            {
                vMoveDist = vDist;
            }

            if (hDist - radius <= 0.1f && Mathf.Abs(vDist) <= 0.1f)
            {
                phase = 1;
            }
        }
        else if (phase == 1) // adjust dir smoothly
        {
            if (cachedTransform.eulerAngles.x <= 1)
            {
                phase = 2;
            }
        }
        else
        {
            if (!triggered)
            {
                triggered = true;
                var magicObjectBowl = magicObject as MagicObjectBowl;
                magicObjectBowl.DestroySelf();
            }

            vMoveDist = ySpeedFactor * moveSpeed * deltaTime;
        }

        newPos.y += vMoveDist;
        Vector3 newDir = newPos - fishPosition;
        Quaternion targetRot = Quaternion.LookRotation(newDir, Vector3.up);
        var newRot = Quaternion.Lerp(cachedTransform.rotation, targetRot, deltaTime * 6); // avoid jittery
        cachedTransform.rotation = newRot;
        cachedTransform.position = newPos;
    }

    private Vector3 RotateVectorAroundUp(Vector3 direction, float angle)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 rotatedDirection = rotation * direction;
        return rotatedDirection;
    }
}