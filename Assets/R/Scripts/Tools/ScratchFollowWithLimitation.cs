using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScratchFollowWithLimitation : ObjectFollowScratch
{
    [SerializeField] public Vector2 TopRight = new Vector2(-1, 1);
    [SerializeField] public Vector2 BottomLeft = new Vector2(1, -1);

    Vector2 xPair; //(min,max)
    Vector2 yPair; //(min,max)

    void Start()
    {
        xPair = TopRight.x > BottomLeft.x ? new Vector2(BottomLeft.x, TopRight.x) : new Vector2(TopRight.x, BottomLeft.x);
        yPair = TopRight.y > BottomLeft.y ? new Vector2(BottomLeft.y, TopRight.y) : new Vector2(TopRight.y, BottomLeft.y);
    }


    protected override void OnUserScratching(Vector2 force)
    {
        Vector3 nextPosition = this.transform.position + new Vector3(-force.x, -force.y) * movementScaler;
        this.transform.position = new Vector3(Mathf.Clamp(nextPosition.x, xPair.x, xPair.y), Mathf.Clamp(nextPosition.y, yPair.x, yPair.y), nextPosition.z);
    }
}