using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class SnapCallback : UnityEvent<int> { }

public class SnapScrollView : ScrollRect
{
    [SerializeField] protected RectTransform snapRect;
    [SerializeField] protected float snapDuration = 0.6f;
    [SerializeField] protected bool snapDurationByDistance = true;
    [SerializeField] protected UnityEvent onBeginDragCallback;
    [SerializeField] protected SnapCallback snappedCellCallback;

    protected bool isSnapping = false;
    protected Vector3 snapTarget = Vector3.zero;
    protected Vector3 snapFrom = Vector3.zero;
    protected int targetFocusCell = -1;

    protected float snapTime = 0;

    public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        if (onBeginDragCallback != null)
        {
            onBeginDragCallback.Invoke();
        }

        targetFocusCell = -1;
        isSnapping = false;
    }

    public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        velocity = Vector2.zero;
        SnapCalculate();
    }

    public void SnapToIndex(int index, bool isImmediately = false)
    {
        targetFocusCell = index;
        SnapToPosition(content.GetChild(index), isImmediately);
    }

    public void SnapCalculate()
    {
        int nearestCell = 0;
        float distance = 10000;

        Vector3 target = viewport.position;

        for (int i = 0; i < content.childCount; i++)
        {
            float d = Vector3.Distance((content.GetChild(i) as RectTransform).position, target);
            if (d <= distance)
            {
                nearestCell = i;
                distance = d;
            }
        }

        targetFocusCell = nearestCell;

        SnapToPosition(content.GetChild(nearestCell));
    }

    void SnapToPosition(Transform target, bool isImmediately = false)
    {
        if (isImmediately)
        {
            snapTarget = -content.InverseTransformPoint(target.position);
            content.anchoredPosition3D = snapTarget;

            if (targetFocusCell != -1 && snappedCellCallback != null)
            {
                snappedCellCallback.Invoke(targetFocusCell);
            }
        }
        else
        {
            snapTarget = -content.InverseTransformPoint(target.position);
            snapFrom = content.anchoredPosition3D;

            float dis = Vector3.Distance(snapTarget, snapFrom);

            snapTime = snapDurationByDistance ? dis / (target as RectTransform).sizeDelta.x * snapDuration : snapDuration;
            timeCount = 0;

            isSnapping = true;
        }
    }

    float timeCount = 0;

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (isSnapping)
        {
            if (timeCount < snapTime)
            {
                content.anchoredPosition3D = Vector3.Lerp(snapFrom, snapTarget, timeCount / snapTime);
                timeCount += Time.deltaTime;
            }
            else
            {
                content.anchoredPosition3D = snapTarget;
                isSnapping = false;

                if (targetFocusCell != -1 && snappedCellCallback != null)
                {
                    snappedCellCallback.Invoke(targetFocusCell);
                }
            }
        }
    }
}