using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class UIDataUpdater : MonoBehaviour
{
    [SerializeField] protected Image hpBar;
    [SerializeField] protected Text hpDisplay;
    [SerializeField] protected Text scoreText;
    [SerializeField] protected Text timeText;
    [SerializeField] protected Text taskProgressText;
    [SerializeField] protected Image taskProgressBar;

    [SerializeField] protected GameObject taskObject;
    [SerializeField] protected GameObject gameoverObject;

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnHpPercentageUpdated, OnHpPercentageUpdated);
        EventManager.StartListening(EventList.OnScoreUpdated, OnScoreUpdated);
        EventManager.StartListening(EventList.OnTimeUpdated, OnTimeUpdated);
        EventManager.StartListening(EventList.OnTaskProgressUpdated, OnTaskProgressUpdated);
        EventManager.StartListening(EventList.OnGameStatusChanged, OnGameStatusChanged);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnHpPercentageUpdated, OnHpPercentageUpdated);
        EventManager.StopListening(EventList.OnScoreUpdated, OnScoreUpdated);
        EventManager.StopListening(EventList.OnTimeUpdated, OnTimeUpdated);
        EventManager.StopListening(EventList.OnTaskProgressUpdated, OnTaskProgressUpdated);
        EventManager.StopListening(EventList.OnGameStatusChanged, OnGameStatusChanged);
    }

    void Start()
    {
        OnScoreUpdated(0);
        OnTimeUpdated(0);
    }

    protected void OnHpPercentageUpdated(object result)
    {
        BarValue value = (BarValue)result;

        hpBar.fillAmount = value.Percentage;
        hpDisplay.text = string.Format("{0}/{1}", value.CurrentValue, value.FullValue);
    }

    protected void OnScoreUpdated(object result)
    {
        scoreText.text = ((int)result).ToString();
    }

    protected void OnTimeUpdated(object result)
    {
        int seconds = (int)result;

        timeText.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
    }

    protected void OnTaskProgressUpdated(object result)
    {
        InGameTaskData taskData = (InGameTaskData)result;
        if (taskData != null)
        {
            taskProgressText.text = Localization.Get("txt_task_progress") + " : " + taskData.DisplayProgress;
            Localization.SetFont(taskProgressText);
            taskProgressBar.fillAmount = taskData.Progress;
        }
        else
        {
            taskObject.SetActive(false);
        }
    }

    public void OnPauseButtonClick()
    {
        Hashtable hashtable = new Hashtable();
        hashtable[PopupProperty.Prefab] = PopupManager.Instance.GetPopupPrefab(PopupPrefab.PausePopup);

        PopupManager.Instance.AddPopupToQueueAndShow(hashtable);
    }

    public void OnGameStatusChanged(object result)
    {
        bool isAlive = (bool)result;

        gameoverObject.SetActive(!isAlive);
    }
}