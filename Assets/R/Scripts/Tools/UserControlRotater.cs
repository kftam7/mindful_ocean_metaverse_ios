using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControlRotater : MonoBehaviour
{
    protected float fromRotation = 0;
    protected float targetRotation = 0;
    protected float rotateProgress = 0;
    protected float rotateSpeed = 4f;
    protected float arRatio = 180f / Mathf.PI;


    protected virtual void OnEnable()
    {
        UserControlObject.OnDirectionChanged += OnTargetAngleChanged;
    }

    protected virtual void OnDisable()
    {
        UserControlObject.OnDirectionChanged -= OnTargetAngleChanged;
    }

    void Update()
    {
        rotateProgress = rotateProgress + Time.deltaTime * rotateSpeed;
        this.transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(fromRotation, targetRotation, rotateProgress));
    }

    protected virtual void OnTargetAngleChanged(Vector2 force)
    {
        float angle = GetAngle(force);

        fromRotation = this.transform.eulerAngles.z;
        targetRotation = angle;

        rotateProgress = 0;
    }

    //return 0~360
    public virtual float GetAngle(Vector2 force)
    {
        float angle = Mathf.Atan(force.y / force.x);
        float result;

        if (force.x >= 0)
        {
            result = angle * arRatio;
        }
        else
        {
            result = 180f + angle * arRatio;
        }

        return result >= 0 ? result : result + 360;
    }
}