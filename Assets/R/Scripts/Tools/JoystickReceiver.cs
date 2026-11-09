using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class JoystickReceiver : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        Joystick.OnUserTouchingJoystick += OnUserTouchingJoystick;
    }

    protected virtual void OnDisable()
    {
        Joystick.OnUserTouchingJoystick -= OnUserTouchingJoystick;
    }

    protected abstract void OnUserTouchingJoystick(Vector2 force);
}