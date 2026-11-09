using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UserControlReceiver : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        ScratchInput.OnUserScratching += OnUserScratching;
    }

    protected virtual void OnDisable()
    {
        ScratchInput.OnUserScratching -= OnUserScratching;
    }

    protected abstract void OnUserScratching(Vector2 force);
}