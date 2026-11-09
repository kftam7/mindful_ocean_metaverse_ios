using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBlocker : GeneralTutorialProgress
{
    public override void StartProgress(TutorialStep step)
    {
        base.StartProgress(step);
        StartCoroutine(AutoEnd(mCurrentTutorialProgress.BlockTime));
    }

    IEnumerator AutoEnd(float time)
    {
        yield return new WaitForSeconds(time);
        base.OnTutorialProgressEnd();
    }
}