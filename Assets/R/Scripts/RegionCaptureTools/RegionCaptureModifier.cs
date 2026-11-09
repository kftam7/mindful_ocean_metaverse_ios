using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RegionCaptureModifier : ScratchInput
{
    [SerializeField] protected Transform target;
    [SerializeField] protected float MaxDisplacement = 1;
    [SerializeField] protected float forceConstant = 1;

    protected Vector3 originPosition;

    void Start()
    {
        originPosition = target.position;
    }

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnTrackingFound, OnTrackingFound);
        EventManager.StartListening(EventList.OnTrackingLost, OnTrackingLost);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnTrackingFound, OnTrackingFound);
        EventManager.StopListening(EventList.OnTrackingLost, OnTrackingLost);
    }

    protected void OnTrackingFound(object result)
    {
        target.position = originPosition;
    }

    protected void OnTrackingLost(object result)
    {
        target.position = originPosition;
    }

    Vector2 increasePosition = Vector2.zero;

    public override void UserScratching(Vector2 dif)
    {
        dif *= -forceConstant;

        increasePosition.x = Mathf.Max(Mathf.Min(increasePosition.x + dif.x, MaxDisplacement), -MaxDisplacement);
        increasePosition.y = Mathf.Max(Mathf.Min(increasePosition.y + dif.y, MaxDisplacement), -MaxDisplacement);

        target.position = new Vector3(increasePosition.x, 0, increasePosition.y);
    }
}