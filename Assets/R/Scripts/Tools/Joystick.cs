using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnUserInputEvent(Vector2 force);

public class Joystick : MonoBehaviour
{
    [SerializeField] public float MaxMoveLength = 0; //no limitation
    [SerializeField] public float MinMoveLength = 0; //no limitation

    public static event OnUserInputEvent OnUserTouchingJoystick;

    protected Vector3 startPosition;
    protected bool activatedJoystick = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (MaxMoveLength >= MinMoveLength)
            {
                startPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                activatedJoystick = true;
            }
            else
            {
                Debug.LogError("Config Error");
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            activatedJoystick = false;
        }

        if (activatedJoystick)
        {
            Vector2 inputForce = EvaluateUserInput(Camera.main.ScreenToViewportPoint(Input.mousePosition) - startPosition);
            if (OnUserTouchingJoystick != null)
            {
                OnUserTouchingJoystick(inputForce);
            }
        }
    }

    protected Vector2 EvaluateUserInput(Vector3 dif)
    {
        Vector2 difV2 = new Vector2(dif.x, dif.y);

        float magnitude = difV2.magnitude;

        if (MaxMoveLength <= 0 && MinMoveLength <= 0)
        {
            return difV2;
        }

        if (MinMoveLength > 0 && magnitude < MinMoveLength)
        {
            return Vector2.zero;
        }

        if (MaxMoveLength > 0 && magnitude > MaxMoveLength)
        {
            return difV2 / magnitude * MaxMoveLength;
        }

        return difV2;
    }
}