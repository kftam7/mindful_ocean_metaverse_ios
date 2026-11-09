using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskData
{
    public int Id { get; set; }
    public string Character { get; set; }
    public string TaskName { get; set; }
    public string Description { get; set; }
    public string RefDescription { get; set; }
    public TaskTarget Target { get; set; }
    public int TargetAmount { get; set; }
    public string RewardDescription { get; set; }
    public int Reward { get; set; }
    public int TaskIndex { get; set; }
    public bool IsLastTask { get; set; }

    public string TargetDescription
    {
        get { return string.Format(ProGamer.Localization.Get(Description), TargetAmount); }
    }
}