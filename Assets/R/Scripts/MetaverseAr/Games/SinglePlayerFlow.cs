using System;
using System.Collections.Generic;
using System.Linq;
using Auki.ConjureKit;
using UnityEngine;

public class SinglePlayerFlow : SystemController, IGameFlow
{
    public event Action<ARGameState> OnStateChange;
    public event Action<int> OnTimerUpdate;

    private ARGameState _gameState;
    private BottleSystem _bottleSystem;
    private FishEntity _myFish;
    private int _bottleCollectedPreviously;
    private double _startTime;
    private double _sessionDuration;
    private readonly bool _isHost;
    private readonly BottleController _bottleController;
    private const string tipPath = "BottleGame/GameTips";
    private readonly string[] _tipsResName = new string[3] { "MoveFish", "LeadFish", "CollectBottles" };

    private Queue<TipQueue> _tipQueues = new();
    private string _domainId;
    private string _domainName;

    public Dictionary<uint, int> PlayerScores { get; private set; } = new();

    public event Action<uint, int> OnPlayerScore;
    public event Action OnLimitAchieved;
    public event Action OnAnimateCelebration;
    public event Action<Queue<TipQueue>> OnShowTip;

    public SinglePlayerFlow(BottleController bottleController, bool host)
    {
        _isHost = host;
        _bottleController = bottleController;

        _tipQueues = ((IGameFlow)this).InitializeTip(tipPath, _tipsResName);
    }

    public void StartGame(Session session, string domainId, string domainName)
    {
        _startTime = Time.timeAsDouble;
        _domainId = domainId;
        _domainName = domainName;
        ARAquariumUIController.OnLeaveGame += Deinitialize;
        AukiSdkWrapper.Instance.LeftEvent += EndGame;
        Initialize(session);
        _gameState = ARGameState.GameInSession;
        OnTimerUpdate?.Invoke(-1);
        OnStateChange?.Invoke(_gameState);

        if (ServerTimeHelper.HasDayPassed())
        {
            GameSave.SinglePlayerModeLimitReached = false;
        }

        if (GameSave.SinglePlayerModeLimitReached)
            OnLimitAchieved?.Invoke();

        _bottleCollectedPreviously = BottlesCollection.SinglePlayerBottlesCollected;

        OnShowTip?.Invoke(_tipQueues);
    }

    private void EndGame()
    {
        Deinitialize();
    }

    private void Initialize(Session session)
    {
        _session = session;
        _bottleSystem = new BottleSystem(session);
        RegisterSystem(_bottleSystem, () =>
        {
            _bottleSystem.GetComponentsTypeId();
            _bottleController.Setup(5, _bottleSystem, _session, _isHost);
            _bottleSystem.OnPlayerScore += PlayerScore;
            InitializeScore();
        });

        _myFish = ARMetaverseManager.Instance.MyFish;
    }

    private void InitializeScore()
    {
        _bottleSystem.InitializeScore(_myFish.AukiEntity.Id, _myFish.AukiEntity.ParticipantId);
    }

    private void Deinitialize()
    {
        _bottleController.CleanUp();
        if (_bottleSystem != null)
        {
            _bottleSystem.OnPlayerScore -= PlayerScore;
            if (_myFish.AukiEntity != null)
                _bottleSystem.DeleteScoreComponent(_myFish.AukiEntity);
        }
        _bottleSystem = null;
        _session = null;
        _gameState = ARGameState.GameEnd;
        OnStateChange?.Invoke(_gameState);
        LogDuration();
        ARAquariumUIController.OnLeaveGame -= Deinitialize;
        AukiSdkWrapper.Instance.LeftEvent -= EndGame;
    }

    private void PlayerScore(uint playerId, int score)
    {
        var combinedScore = _bottleCollectedPreviously + score;
        PlayerScores[playerId] = score;
        OnPlayerScore?.Invoke(playerId, score);

        if (combinedScore == GameController.GameConfig.MaxBottlePerDay)
        {
            OnLimitAchieved?.Invoke();
            OnAnimateCelebration?.Invoke();
            GameSave.SinglePlayerModeLimitReached = true;
            ServerTimeHelper.UpdateLastSessionDateTime();
            TipEventTrigger.TriggerEvent(TipEvent.Collect15Bottles);
        }
    }

    private void LogDuration()
    {
        var userId = GameController.PlayerData.appleUserId;
        _sessionDuration = (Time.time - _startTime) / 60;
        Analytices.Instance.DomainSessionDuration(_domainName, _domainId, userId, _sessionDuration);
    }
}