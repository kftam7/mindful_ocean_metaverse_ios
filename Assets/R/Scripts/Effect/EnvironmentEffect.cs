using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentEffect : UnderwaterEffect
{
    protected bool isUnderwater = true;

    protected float WaterLevel
    {
        get { return WaterLevelBoundary.Instance.WaterLevel; }
    }

    protected override void Start()
    {
        base.Start();

        SetUnderwaterEffect();
    }

    protected override void Update()
    {
        //status changed
        bool currentIsUnderwater = this.transform.position.y < WaterLevel;

        if (currentIsUnderwater != isUnderwater)
        {
            if (isUnderwater)
            {
                LerpUnderwaterEffect();
            }
            else
            {
                LerpNormalEffect();
            }
        }

        base.Update();
    }
}