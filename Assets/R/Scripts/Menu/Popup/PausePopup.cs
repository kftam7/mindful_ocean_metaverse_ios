using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePopup : ProGamer.SimplePopup
{
    [SerializeField] protected GameObject taskMenu;
    [SerializeField] protected Text taskDescription;
    [SerializeField] protected Text taskProgressText;
    [SerializeField] protected Image taskProgressBar;

    protected override void InitPopup()
    {
        base.InitPopup();

        InGameTaskData data = InGameData.Instance.Task;
        if (data != null)
        {
            taskMenu.SetActive(true);
            SetTaskData(data);
        }
        else
        {
            taskMenu.SetActive(false);
        }

        Pause(true);
    }

    protected void SetTaskData(InGameTaskData data)
    {
        taskDescription.text = data.TaskStaticData.TargetDescription;
        taskProgressText.text = string.Format("{0}/{1}", data.CurrentProgress, data.TaskStaticData.TargetAmount);
        taskProgressBar.fillAmount = (float)data.CurrentProgress / data.TaskStaticData.TargetAmount;
    }

    public void Resume()
    {
        Pause(false);
        base.Close();
    }

    public void GoMainMenu()
    {
        Pause(false);
#if EVENT_DEMO
		SceneTransitHandler.LoadScene("FeedbackScene");
#else
        SceneTransitHandler.BackMainMenu();
#endif
    }

    public void GiveUp()
    {
        InGameData.Instance.GotDamage(100);
        Resume();
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}