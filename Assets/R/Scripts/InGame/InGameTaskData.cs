using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameTaskData
{
    public TaskData TaskStaticData { get; set; }

    public int CurrentProgress { get; set; }

    public bool IsFinished
    {
        get { return this.CurrentProgress / this.TaskStaticData.TargetAmount >= 1; }
    }

    public float Progress
    {
        get { return (float)this.CurrentProgress / this.TaskStaticData.TargetAmount; }
    }

    public string DisplayProgress
    {
        get { return string.Format("{0}/{1}", CurrentProgress, TaskStaticData.TargetAmount); }
    }

    public InGameTaskData(TaskData data, int progress)
    {
        this.TaskStaticData = data;
        this.CurrentProgress = progress;
    }

    public void GainProgress(int amount)
    {
        CurrentProgress = Mathf.Min(this.TaskStaticData.TargetAmount, CurrentProgress + amount);
    }
}