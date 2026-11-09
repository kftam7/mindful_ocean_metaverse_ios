using System;
using UnityEngine;
using UnityEngine.UI;

public class GameSessionView : BaseView<object>
{
    [SerializeField] private Text dailyCollectText;
    [SerializeField] private GameObject dailyCollectGroup;
    [SerializeField] private Text totalCollectText;
    [SerializeField] private Text matchTodayText;
    [SerializeField] private GameObject matchTodayGroup;

    private int _singlePlayerModeBottlesCollected;
    private int _totalCollected;

    public override void Show(object data)
    {
        _singlePlayerModeBottlesCollected = BottlesCollection.SinglePlayerBottlesCollected;
        _totalCollected = BottlesCollection.TotalBottlesCollected;
        GameController.OnUpdateScore += UpdateScore;
        dailyCollectText.gameObject.SetActive(true);
        totalCollectText.gameObject.SetActive(true);
        matchTodayText.gameObject.SetActive(true);
        dailyCollectGroup.SetActive(GameController.IsSinglePlayer);
        matchTodayGroup.SetActive(!GameController.IsSinglePlayer);

        UpdateScoreText(0);
        UpdateMatchTotal(Mathf.Min(GameSave.MultiPlayerModeMatchCount + 1, GameController.GameConfig.MaxMatchPerDay));

        base.Show(data);

        mainPanel.GetComponent<RectTransform>().RefreshLayoutGroupsImmediateAndRecursive();
    }

    public override void Hide()
    {
        GameController.OnUpdateScore -= UpdateScore;
        dailyCollectText.gameObject.SetActive(false);
        totalCollectText.gameObject.SetActive(false);
        base.Hide();
    }

    void OnDestroy()
    {
        GameController.OnUpdateScore -= UpdateScore;
    }

    private void UpdateScore(uint participantId, int score)
    {
        Logger.Log($"[Score] Scoring PlayerId: {participantId}, local playerId: {AukiSdkWrapper.Instance.MyPeerId}");
        if (participantId != AukiSdkWrapper.Instance.MyPeerId) return;
        if (GameController.IsSinglePlayer && GameSave.SinglePlayerModeLimitReached) return;
        if (!GameController.IsSinglePlayer && GameSave.MultiPlayerModeLimitReached) return;

        UpdateScoreText(score);
    }

    private void UpdateScoreText(int score)
    {
        dailyCollectText.text = $"{_singlePlayerModeBottlesCollected + score}/{GameController.GameConfig.MaxBottlePerDay}";
        totalCollectText.text = (_totalCollected + score).ToString();
    }

    private void UpdateMatchTotal(int match)
    {
        matchTodayText.text = $"{match}/{GameController.GameConfig.MaxMatchPerDay}";
    } 
}