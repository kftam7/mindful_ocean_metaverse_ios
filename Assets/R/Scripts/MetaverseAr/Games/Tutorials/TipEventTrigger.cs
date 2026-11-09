using System;

public static class TipEventTrigger
{
    public static event Action<TipEvent> OnEventTrigger;

    public static void TriggerEvent(TipEvent tipEvent)
    {
        OnEventTrigger?.Invoke(tipEvent);
    }
}