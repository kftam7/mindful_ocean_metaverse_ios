using System;
using System.Collections.Generic;
using Game.Models;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class MasterGameView : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private MessageBoxView messageBoxView;
    [SerializeField] private GameSessionView gameSessionView;
    [SerializeField] private GameOverView gameOverView;
    [SerializeField] private TipView tipView;
    [SerializeField] private GameObject whiteTintBg;

    [Header("Debug")]
    [SerializeField] private Text debugText;
    private ARGameState _state;
    private Action closePopup;

    void Start()
    {
        whiteTintBg.SetActive(false);
        messageBoxView.Hide();
        gameSessionView.Hide();
        UpdateTimer(-1);

        GameController.OnLimitAchieved += LimitAchieved;
    }

    void OnDestroy()
    {
        GameController.OnLimitAchieved -= LimitAchieved;
    }

    private void LimitAchieved()
    {
        ShowMessage(GameController.IsSinglePlayer ? "txt_single_mode_reach_limit" : "txt_multi_mode_reach_limit");
    }

    public void OSDDebug(string message)
    {
        debugText.text = message;
    }

    public void OnChangeState(ARGameState state)
    {
        _state = state;
        switch (state)
        {
            case ARGameState.Waiting:
                whiteTintBg.SetActive(true);
                gameSessionView.Hide();
                break;
            case ARGameState.GameInSession:
                CloseHintPopup();
                whiteTintBg.SetActive(false);
                gameSessionView.Show(null);
                break;
            case ARGameState.GameEnd:
                CloseHintPopup();
                whiteTintBg.SetActive(false);
                gameSessionView.Hide();
                break;
        }
    }

    public void UpdateTimer(int timer)
    {
        timerText.text = timer > -1 ? timer.ToString("00") : string.Empty;
    }

    public void ShowTips(Queue<TipQueue> tips)
    {
        tipView.QueueTips(tips);
    }

    public void ShowMessage(string key)
    {
        messageBoxView.Show(new DialogObject()
        {
            key = Localization.Get(key),
            noDialog = true
        });
    }

    public void ShowLowerHintPopup(string message, float delay = -1f)
    {
        closePopup?.Invoke();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
        var popup = PopupManager.Instance.ActivePopup as HintPopup;
        popup.SetTextLower(message);
        closePopup = () => popup.Close();
        if (delay > 0f)
            popup.SetDelayClose(delay);
    }

    public void CloseHintPopup()
    {
        closePopup?.Invoke();
    }

    public void ShowDialogBox(string key, Action acceptAction, Action cancelAction = null, string yesKey = null, string noKey = null)
    {
        messageBoxView.Show(new DialogObject()
        {
            key = key,
            yesAction = acceptAction,
            yesKey = yesKey,
            noAction = cancelAction,
            noKey = noKey,
            noDialog = false
        });
    }

    public void ShowScoresSummary(ScoreSummary data)
    {
        gameOverView.Show(data);
    }
}