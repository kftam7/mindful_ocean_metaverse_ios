using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskPopup : ProGamer.SimplePopup
{
    [SerializeField] protected Text taskDescription;
    [SerializeField] protected Text taskProgressText;
    [SerializeField] protected Image taskProgressBar;
    [SerializeField] protected Text rewardText;

    protected override void InitPopup()
    {
        base.InitPopup();

        InGameTaskData data = (InGameTaskData)this.parameters["Task"];
        SetTaskData(data);
    }

    protected void SetTaskData(InGameTaskData data)
    {
        taskDescription.text = data.TaskStaticData.TargetDescription;
        taskProgressText.text = string.Format("{0}/{1}", data.CurrentProgress, data.TaskStaticData.TargetAmount);
        taskProgressBar.fillAmount = (float)data.CurrentProgress / data.TaskStaticData.TargetAmount;
        rewardText.text = data.TaskStaticData.RewardDescription;
    }

    public void EnterGame()
    {
        SceneTransitHandler.LoadScene("GameScene");
    }
}