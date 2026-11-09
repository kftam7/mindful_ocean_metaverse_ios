using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TutorialProgressCallback : UnityEvent<TutorialStep> { };

public class GeneralTutorialProgress : MonoBehaviour
{
    public TutorialProgressCallback TutorialProgressStartCallback;
    public TutorialProgressCallback TutorialProgressEndCallback;
    public TutorialProgressCallback TutorialProgressPauseCallback;
    public TutorialProgressCallback TutorialProgressTerminatedCallback;
    public TutorialProgressCallback TutorialProgressSkippedCallback;

    protected TutorialStep mCurrentTutorialProgress;

    public virtual void StartProgress(TutorialStep step)
    {
        mCurrentTutorialProgress = step;
    }

    public virtual void OnTutorialProgressStart()
    {
        if (mCurrentTutorialProgress != null && TutorialProgressStartCallback != null)
        {
            TutorialProgressStartCallback.Invoke(mCurrentTutorialProgress);
        }
    }

    public virtual void OnTutorialProgressEnd()
    {
        if (mCurrentTutorialProgress != null && TutorialProgressEndCallback != null)
        {
            TutorialProgressEndCallback.Invoke(mCurrentTutorialProgress);
        }
    }

    public virtual void OnTutorialProgressPause()
    {
        if (mCurrentTutorialProgress != null && TutorialProgressPauseCallback != null)
        {
            TutorialProgressPauseCallback.Invoke(mCurrentTutorialProgress);
        }
    }

    public virtual void OnTutorialProgressTerminatedCallback()
    {
        if (mCurrentTutorialProgress != null && TutorialProgressTerminatedCallback != null)
        {
            TutorialProgressTerminatedCallback.Invoke(mCurrentTutorialProgress);
        }
    }

    public virtual void OnTutorialProgressSkippedCallback()
    {
        if (mCurrentTutorialProgress != null && TutorialProgressSkippedCallback != null)
        {
            TutorialProgressSkippedCallback.Invoke(mCurrentTutorialProgress);
        }
    }
}