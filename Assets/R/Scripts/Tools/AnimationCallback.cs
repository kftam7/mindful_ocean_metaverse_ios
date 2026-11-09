using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class AnimationCallback : MonoBehaviour
{
    [SerializeField] protected UnityEvent[] Callbacks;

    public void Execute()
    {
        ExecuteCallback(0);
    }

    public void ExecuteCallback(int index)
    {
        if (Callbacks.Length > index)
        {
            if (Callbacks[index] != null) Callbacks[index].Invoke();
        }
    }
}