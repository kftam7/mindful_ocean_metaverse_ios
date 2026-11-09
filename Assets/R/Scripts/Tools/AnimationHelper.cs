using UnityEngine;

public static class AnimationHelper
{
    public static void AddEvent(this AnimationClip clip, string functionName, int parameter, float time)
    {
        AnimationEvent animationEvent = new AnimationEvent();

        animationEvent.intParameter = parameter;
        animationEvent.time = time;
        animationEvent.functionName = functionName;

        clip.AddEvent(animationEvent);
    }

    public static void AddEventAtTheEnd(this AnimationClip clip, string functionName, int parameter)
    {
        AddEvent(clip, functionName, parameter, clip.length);
    }

    public static void AddEventAtTheBeginning(this AnimationClip clip, string functionName, int parameter)
    {
        AddEvent(clip, functionName, parameter, 0f);
    }
}