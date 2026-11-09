using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using ProGamer;

public class AdvanceButton : Button
{
    [System.Serializable]
    public class AdvanceButtonEvent : UnityEvent<AdvanceButton> { }

    [SerializeField] private string onClickSound;

    public string OnClickSound
    {
        get { return onClickSound; }
        set { onClickSound = value; }
    }

    [SerializeField] private AdvanceButtonEvent onPointerClickEvent = new AdvanceButtonEvent();

    public AdvanceButtonEvent OnPointerClickEvent
    {
        get { return onPointerClickEvent; }
        set { onPointerClickEvent = value; }
    }

    [SerializeField] private AdvanceButtonEvent onPointerDownEvent = new AdvanceButtonEvent();

    public AdvanceButtonEvent OnPointerDownEvent
    {
        get { return onPointerDownEvent; }
        set { onPointerDownEvent = value; }
    }

    [SerializeField] private AdvanceButtonEvent onPointerUpEvent = new AdvanceButtonEvent();

    public AdvanceButtonEvent OnPointerUpEvent
    {
        get { return onPointerUpEvent; }
        set { onPointerUpEvent = value; }
    }

    [SerializeField] private AdvanceButtonEvent onPointerEnterEvent = new AdvanceButtonEvent();

    public AdvanceButtonEvent OnPointerEnterEvent
    {
        get { return onPointerEnterEvent; }
        set { onPointerEnterEvent = value; }
    }

    [SerializeField] private AdvanceButtonEvent onPointerExitEvent = new AdvanceButtonEvent();

    public AdvanceButtonEvent OnPointerExitEvent
    {
        get { return onPointerExitEvent; }
        set { onPointerExitEvent = value; }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        OnPointerClickEvent.Invoke(this);
    }


    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnPointerDownEvent.Invoke(this);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnPointerUpEvent.Invoke(this);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnPointerEnterEvent.Invoke(this);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnPointerExitEvent.Invoke(this);
    }
}