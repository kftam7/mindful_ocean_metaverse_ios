using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using Game.Models;
using UnityEngine;

/// <summary>
/// GameController, instantiate IGameFlow, handle GameFlow update
/// Listens to game state changes
/// </summary>
public class GameController : MonoBehaviour
{
    public ARGameState gameState = ARGameState.Null;
    [SerializeField] private MasterGameView gameView;
    [SerializeField] private ParticleSystem particleFireworks;
    [SerializeField] private BottleController bottleController = new();
    [SerializeField] private GameConfig gameConfig;

    #region Private Fields
    private IGameFlow _currentGameFlow;
    private Session _session;
    private bool _isHost;
    private Analytices _analytics;
    #endregion

    private readonly string[] _builtinDomains = new string[] { "Little Adventure", "Ocean Race" };

    public static event Action<ARGameState> OnStateChange;
    public static event Action<uint, int> OnUpdateScore;
    public static event Action OnLimitAchieved;
    public static event Action<bool, int> OnFinishGame;
    public static GameConfig GameConfig;
    private string _domainId;
    private string _domainName;

    public static PlayerData PlayerData { get; private set; }
    public static bool IsSinglePlayer { get; private set; }

    void Start()
    {
        _analytics = Analytices.Instance;
        AukiSdkWrapper.Instance.JoinedEvent += OnJoinedSession;
        AukiSdkWrapper.Instance.OnHostingDomainSession += OnHostingDomain;
        AukiSdkWrapper.Instance.OnJoinedDomainSession += OnJoinDomain;

        ConfigController.OnConfigFetched += ConfigFetched;

        GameConfig = gameConfig;
        PlayerData = GetPlayerData();
    }

    void OnDestroy()
    {
        Deinitialize();
        if (!AukiSdkWrapper.Instance) return;
        AukiSdkWrapper.Instance.JoinedEvent -= OnJoinedSession;
        AukiSdkWrapper.Instance.OnHostingDomainSession -= OnHostingDomain;
        AukiSdkWrapper.Instance.OnJoinedDomainSession -= OnJoinDomain;

        ConfigController.OnConfigFetched -= ConfigFetched;
    }

    private void OnJoinedSession(Session session)
    {
        _session = session;
        _isHost = AukiSdkWrapper.Instance.IsHost;
        PlayerData.participantId = session.ParticipantId;
        Initialize(_session);
    }

    private void OnLeftSession()
    {
        if (gameState is not ARGameState.GameEnd)
        {
            gameView.ShowMessage("txt_session_left");
        }
    }

    private void OnHostingDomain(string domainId, string domainName)
    {
        _domainId = domainId;
        _domainName = domainName;
        Deinitialize();
        _isHost = AukiSdkWrapper.Instance.IsHost;

        if (domainName.Equals(_builtinDomains[1], StringComparison.CurrentCultureIgnoreCase))
        {
            _currentGameFlow = new MultiPlayerFlow(
                bottleController,
                _isHost,
                gameView.ShowDialogBox,
                UpdateWaitMessage,
                gameView.ShowScoresSummary);

            _analytics.MultiPlayerDomainEvent(PlayerData.appleUserId);
        }
        else
        {
            _currentGameFlow = new SinglePlayerFlow(bottleController, _isHost);
            _analytics.SinglePlayerDomainEvent(PlayerData.appleUserId);
        }

        if (_session != null && _isHost) Initialize(_session);
    }

    private void OnJoinDomain(string domainId, string domainName)
    {
        Deinitialize();
        _isHost = false;
        _currentGameFlow = new MultiPlayerFlow(
            bottleController,
            _isHost,
            gameView.ShowDialogBox,
            UpdateWaitMessage,
            gameView.ShowScoresSummary);

        _analytics.MultiPlayerDomainEvent(PlayerData.appleUserId);
        if (_session != null) Initialize(_session);
    }

    private void ConfigFetched(GameConfig config)
    {
        GameConfig = gameConfig = config;
    }

    private void UpdateWaitMessage(string message, int timer)
    {
        gameView.ShowLowerHintPopup(message, timer);
    }

    private void UpdateTimer(int timer)
    {
        gameView.UpdateTimer(timer);
    }

    void Initialize(Session session)
    {
        if (_currentGameFlow == null) return;
        _currentGameFlow.OnStateChange += UpdateState;
        _currentGameFlow.OnPlayerScore += OnPlayerScore;
        _currentGameFlow.OnTimerUpdate += UpdateTimer;
        _currentGameFlow.OnLimitAchieved += LimitAchieved;
        _currentGameFlow.OnAnimateCelebration += PlayCelebrationFx;
        _currentGameFlow.OnShowTip += ShowTip;
        AukiSdkWrapper.Instance.LeftEvent += OnLeftSession;

        IsSinglePlayer = _currentGameFlow is SinglePlayerFlow;

        _currentGameFlow.StartGame(session, _domainId, _domainName);
    }

    private void ShowTip(Queue<TipQueue> queue)
    {
        gameView.ShowTips(queue);
    }

    private void OnPlayerScore(uint playerId, int score)
    {
        Logger.Log($"Fish picked bottle: {playerId}, score: {score}");
        OnUpdateScore?.Invoke(playerId, score);
    }

    private void SendBottlesCollected()
    {
        Logger.Log($"[Collection] Bottle collected {_session.ParticipantId}");
        foreach (var item in _currentGameFlow.PlayerScores)
        {
            Logger.Log($"[Collection] Player ID: {item.Key}, Score collected: {item.Value}");
        }
        if (_currentGameFlow.PlayerScores.TryGetValue(_session.ParticipantId, out var collected))
        {
            OnFinishGame?.Invoke(IsSinglePlayer, collected);
        }
    }

    void Deinitialize()
    {
        if (_currentGameFlow == null) return;
        _currentGameFlow.OnStateChange -= UpdateState;
        _currentGameFlow.OnPlayerScore -= OnPlayerScore;
        _currentGameFlow.OnTimerUpdate -= UpdateTimer;
        _currentGameFlow.OnLimitAchieved -= LimitAchieved;
        _currentGameFlow.OnAnimateCelebration -= PlayCelebrationFx;
        _currentGameFlow = null;
        if (AukiSdkWrapper.Instance == null) return;
        AukiSdkWrapper.Instance.LeftEvent -= OnLeftSession;
        gameState = ARGameState.Null;
        AukiSdkWrapper.Instance?.LeaveDomain();
        AukiSdkWrapper.Instance?.Disconnect();
        AukiSdkWrapper.Instance?.Connect();
    }

    private void LimitAchieved()
    {
        SendBottlesCollected();
        OnLimitAchieved?.Invoke();
    }

    private void PlayCelebrationFx()
    {
        particleFireworks?.Play();
    }

    private void UpdateState(ARGameState state)
    {
        OnStateChange?.Invoke(state);
        switch (state)
        {
            case ARGameState.Waiting:
                break;
            case ARGameState.GameInSession:
                break;
            case ARGameState.GameEnd:
                SendBottlesCollected();
                Deinitialize();
                UpdateTimer(-1);
                break;
        }
        gameView.OnChangeState(state);
        gameState = state;
    }

    public static PlayerData GetPlayerData()
    {
        var data = new PlayerData();
#if UNITY_EDITOR
        var rand = Application.productName.Contains("Mirror") ? 2 : 1;
        data.appleUserId = $"Editor_user_id_{rand}";
        data.appleName = $"Editor_name_{rand}";
        data.appleEmail = $"Editor_email_{rand}@email.com";
#else
        data.appleUserId = ServerCenter.Instance.AppleUserId;
        data.appleName = ServerCenter.Instance.AppleName;
        data.appleEmail = ServerCenter.Instance.AppleEmail;
#endif
        return data;
    }
}