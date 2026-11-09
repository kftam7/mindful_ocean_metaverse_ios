using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleStateMachine<T>
{
    private T _currentState;
    public System.Action<T> OnStateEnter = delegate { };
    public System.Action<T> OnStateExit = delegate { };

    public void SwitchState(T state)
    {
        OnStateExit(_currentState);
        _currentState = state;
        OnStateEnter(_currentState);
    }
}