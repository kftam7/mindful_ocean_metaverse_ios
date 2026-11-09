using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//Must attached with UI
public class ScratchInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public static event OnUserInputEvent OnUserScratching;

    public UnityEvent OnClick;

    [SerializeField]
    protected float minMoveDistance = 5f;

    protected Vector2 lastPosition;
    protected bool activatedInput = false;
    protected bool isDrag = false;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        activatedInput = true;
        isDrag = false;
        lastPosition = eventData.position;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        activatedInput = false;
        if (!isDrag && OnClick != null)
        {
            OnClick.Invoke();
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (activatedInput)
        {
            Vector2 dif = eventData.position - lastPosition;

            if (dif.magnitude >= minMoveDistance)
            {
                lastPosition = eventData.position;
                isDrag = true;
                UserScratching(dif);
            }
        }
    }

    public virtual void UserScratching(Vector2 dif)
    {
        if (OnUserScratching != null)
        {
            OnUserScratching(dif);
        }
    }
}