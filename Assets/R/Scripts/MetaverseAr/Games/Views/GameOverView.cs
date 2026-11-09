using System.Collections.Generic;
using System.Linq;
using Game.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : BaseView<ScoreSummary>
{
    private const string KeyGameOver = "txt_multi_mode_gameover";
    private const string KeyLeaderboard = "txt_multi_mode_leaderboard";
    private const string KeyLbSubheader = "txt_multi_mode_leaderboard_hint";

    [SerializeField] private ScoreEntry[] scoreEntries;
    [SerializeField] private Text headerText;
    [SerializeField] private Text panelHeaderText;
    [SerializeField] private Text subHeaderText;
    [SerializeField] private RectTransform container;
    [SerializeField] private Button okButton;

    private Dictionary<uint, PlayerData> _playersData = new();

    void Start()
    {
        Hide();
        BottleSystem.OnPlayerJoined += OnPlayerJoins;

        okButton.onClick.AddListener(Hide);
    }

    void OnDestroy()
    {
        BottleSystem.OnPlayerJoined -= OnPlayerJoins;
    }

    private void OnPlayerJoins(uint playerId, PlayerData data)
    {
        if (!_playersData.TryAdd(playerId, data))
            _playersData[playerId] = data;
    }

    public override void Show(ScoreSummary data)
    {
        SetupView(data);
        headerText.SetLocalization(KeyGameOver);
        headerText.gameObject.SetActive(true);
        panelHeaderText.SetLocalization(KeyLeaderboard);
        subHeaderText.SetLocalization(KeyLbSubheader);
        base.Show(data);
        mainPanel.GetComponent<RectTransform>().RefreshLayoutGroupsImmediateAndRecursive();
    }

    public override void Hide()
    {
        headerText.gameObject.SetActive(false);
        base.Hide();
    }

    private void SetupView(ScoreSummary data)
    {
        var sortedScore = data.playerScores.OrderByDescending(x => x.Value);

        foreach (var item in scoreEntries)
        {
            item.gameObject.SetActive(false);
        }

        var i = 0;
        foreach (var item in sortedScore)
        {
            var entry = scoreEntries[i];
            var playerName = _playersData[item.Key].appleName;
            var score = item.Value.ToString("00");
            var leftEarly = false;
            data.playersLeftEarly.TryGetValue(item.Key, out leftEarly);
            entry.Set(playerName, score, leftEarly);
            entry.gameObject.SetActive(true);
            i++;
        }
    }
}