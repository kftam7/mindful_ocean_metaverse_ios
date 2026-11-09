using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollowScratch : UserControlReceiver
{
    [SerializeField] protected float movementScaler = 1;

    protected override void OnUserScratching(Vector2 force)
    {
        this.transform.position += new Vector3(-force.x, -force.y) * movementScaler;
    }
}