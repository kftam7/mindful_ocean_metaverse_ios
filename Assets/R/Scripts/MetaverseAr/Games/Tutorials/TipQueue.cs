using UnityEngine;

[CreateAssetMenu(fileName = "TipData", menuName = "Game Data/Game Tip Data")]
public class TipQueue : ScriptableObject
{
    public string messageKey;
    public TipEvent tipEvent;
    private bool _invoked;

    public bool ConditionMet()
    {
        return _invoked;
    }

    public void Initialize()
    {
        _invoked = false;
        TipEventTrigger.OnEventTrigger += EventTriggered;
    }

    public void Deinitialize()
    {
        TipEventTrigger.OnEventTrigger -= EventTriggered;
    }

    private void EventTriggered(TipEvent @event)
    {
        if (@event != tipEvent) return;
        _invoked = true;
    }
}