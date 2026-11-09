using System;

public static class GlobalEvents
{
    public static event Action<int> MainUiScrollToEvent;

    public static void SendMainUiScrollToEvent(int value)
    {
        MainUiScrollToEvent?.Invoke(value);
    }
}