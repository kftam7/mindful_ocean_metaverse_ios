using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum Gesture
{
    None,
    ZoomIn,
    ZoomOut,
    Tap
};

[System.Serializable]
public class GestureEvent : UnityEvent<Gesture> { }

public class TouchGesture : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public GestureEvent OnTouchGestureTrigger;

    protected Gesture gesture = Gesture.None;
    protected bool isDetectionStart = false;
    protected const float minDistance = 1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        gesture = Gesture.Tap;
        isDetectionStart = true;
        distance = -1;
        initDistance = -1;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDetectionStart)
        {
            isDetectionStart = false;

            if (Mathf.Abs(distance - initDistance) >= minDistance)
            {
                gesture = (distance - initDistance) > 0 ? Gesture.ZoomIn : Gesture.ZoomOut;
            }

            if (OnTouchGestureTrigger != null)
            {
                OnTouchGestureTrigger.Invoke(gesture);
            }
        }
    }

    protected Vector2 firstFinger = Vector2.zero;
    protected Vector2 secondFinger = Vector2.zero;
    protected float initDistance = -1;
    protected float distance = -1;

    public void OnDrag(PointerEventData eventData)
    {
        if (isDetectionStart)
        {
            if (Input.touches.Length == 2)
            {
                if (initDistance <= 0)
                {
                    firstFinger = Input.touches[0].position;
                    secondFinger = Input.touches[1].position;

                    initDistance = Vector2.Distance(firstFinger, secondFinger);
                }
                else
                {
                    distance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                }
            }
        }
    }
}