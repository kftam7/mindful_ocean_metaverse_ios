using System;
using System.Collections.Generic;
using System.Threading;
using Auki.ConjureKit;
using Cysharp.Threading.Tasks;
using Game.Models;
using ProGamer;
using UnityEngine;

public class MultiPlayerFlow : SystemController, IGameFlow
{
    #region Events & Delegates
    public event Action<ARGameState> OnStateChange;
    public event Action<int> OnTimerUpdate;
    public event Action<uint, int> OnPlayerScore;
    public event Action OnLimitAchieved;
    public event Action OnAnimateCelebration;
    public event Action<Queue<TipQueue>> OnShowTip;
    private readonly Action<string, Action, Action, string, string> _showDialog;
    private readonly Action<string, int> _onUpdateWaitMessage;
    private readonly Action<ScoreSummary> _showScores;
    #endregion

    #region Classes & Enums
    private ARGameState _gameState;
    private BottleSystem _bottleSystem;
    private FishEntity _myFish;
    private CancellationTokenSource _cancellationTokenSource = new();
    #endregion

    #region ValueType Fields
    private int _timer;
    private int _bottleCollectedPreviously;
    private int _minPlayersToCommence = 2;
    private int _maxPlayers = 4;
    private int _waitForPlayersDuration = 30;
    private int _gameDuration = 90;
    private bool _isHost;
    private string _domainId;
    private string _domainName;
    private double _startTime;
    private double _sessionDuration;
    private uint _myParticipantId;
    #endregion

    #region Collection Fields
    private Queue<TipQueue> _tipQueues = new();
    public Dictionary<uint, int> PlayerScores { get; private set; } = new();
    private List<uint> _playersInSession = new();
    private Dictionary<uint, bool> _playersLeft = new();
    #endregion

    #region Readonly Fields
    private readonly GameStateHandler _gameStateHandler = new();
    private readonly TimerHandler _timerHandler = new();
    private readonly BottleController _bottleController;
    #endregion

    #region Const Fields
    private const string tipPath = "BottleGame/GameTips";
    // private readonly string[] _tipsResName = new string[3]{ "MoveFish", "LeadFish", "CollectBottles" };
    private const string KeyWaitForParticipant = "txt_multi_mode_wait_hint";
    private const string KeyParticipantJoined = "txt_message_friend_join_hint";
    private const string KeyParticipantLeft = "txt_message_friend_left_hint";
    #endregion

    public MultiPlayerFlow(
        BottleController bottleController,
        bool host,
        Action<string, Action, Action, string, string> showDialog,
        Action<string, int> waitCallback,
        Action<ScoreSummary> showScores)
    {
        _bottleController = bottleController;
        _isHost = host;
        _showDialog = showDialog;
        _onUpdateWaitMessage = waitCallback;
        _showScores = showScores;

        _gameStateHandler.Setup(_isHost);
        _gameStateHandler.OnStateUpdate += OnStateUpdated;

#if UNITY_EDITOR
        _gameDuration = 30;
#endif
    }
    ~MultiPlayerFlow()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        HostHandler.HostAppointedEvent -= HostAppointedEvent;
    }

    public void StartGame(Session session, string domainId, string domainName)
    {
        _startTime = Time.timeAsDouble;
        _session = session;
        _domainId = domainId;
        _domainName = domainName;
        _myParticipantId = _session.ParticipantId;
        Initialize(_session);

        ARAquariumUIController.OnLeaveGame += LeaveGame;
        HostHandler.HostAppointedEvent += HostAppointedEvent;
        AukiSdkWrapper.Instance.LeftEvent += LeftEvent;

        if (ServerTimeHelper.HasDayPassed())
            GameSave.MultiPlayerModeLimitReached = false;

        if (GameSave.MultiPlayerModeLimitReached)
            OnLimitAchieved?.Invoke();

        if (!_isHost)
        {
            OnStateUpdated(ARGameState.Waiting);
            return;
        }

        SendUpdateState(ARGameState.Waiting);
        OnTimerUpdate?.Invoke(-1);
        _timerHandler.RunTimer(_waitForPlayersDuration, CommenceGame);
    }

    private void LeftEvent()
    {
        if (_gameState == ARGameState.GameInSession)
        {
            _playersLeft.Add(_myParticipantId, true);
            EndGame();
        }
        else if (_gameState == ARGameState.Waiting)
        {
            CancelWait();
        }
    }

    private void CommenceGame()
    {
        if (!_isHost) return;

        //if no other joiner ask to wait again
        if (_playersInSession.Count < _minPlayersToCommence)
        {
            _showDialog?.Invoke(
                "txt_multi_mode_quit_hint",
                () => _timerHandler.RunTimer(_waitForPlayersDuration, CommenceGame),
                CancelWait,
                "txt_multi_mode_quit_option",
                "txt_back");
            return;
        }

        SendUpdateState(ARGameState.GameInSession);
        _timerHandler.RunTimer(_gameDuration, EndGame);
    }

    private void InitializeScore()
    {
        _bottleSystem.InitializeScore(_myFish.AukiEntity.Id, _myFish.AukiEntity.ParticipantId);
    }

    private void EndGame()
    {
        if (_isHost)
            SendUpdateState(ARGameState.GameEnd);
        else
            OnStateUpdated(ARGameState.GameEnd);
        HostHandler.HostAppointedEvent -= HostAppointedEvent;
        ARAquariumUIController.OnLeaveGame -= LeaveGame;
    }

    private void CancelWait()
    {
        AukiSdkWrapper.Instance.LeftEvent -= LeftEvent;
        LogDuration();
        Deinitialize();
        if (!GameSave.MultiPlayerModeLimitReached)
        {
            ServerTimeHelper.UpdateLastSessionDateTime();
        }
        OnStateChange?.Invoke(ARGameState.GameEnd);
        HostHandler.HostAppointedEvent -= HostAppointedEvent;
    }

    private void LeaveGame()
    {
        ParticipantLeft(_myParticipantId);
        CalculateScores();
        CancelWait();
        if (NetworkMatchHandler.AddMatch())
        {
            OnLimitAchieved?.Invoke();
        }
    }

    private async void Initialize(Session session)
    {
        _bottleSystem = new BottleSystem(session);

        RegisterSystem(_bottleSystem, async () =>
        {
            _bottleSystem.GetComponentsTypeId();
            _bottleSystem.OnPlayerScore += PlayerScore;
            await UniTask.WaitUntil(() => ARMetaverseManager.Instance.MyFish != null, cancellationToken: _cancellationTokenSource.Token);
            _myFish = ARMetaverseManager.Instance.MyFish;
            await UniTask.WaitUntil(() => _myFish.AukiEntity != null, cancellationToken: _cancellationTokenSource.Token);
            _bottleSystem.AddPlayerData(_myFish.AukiEntity.Id, GameController.PlayerData);
            _bottleSystem.GetAllPreviousPlayers();
        });

        BottleSystem.OnPlayerJoined += ParticipantsJoins;
        AukiSdkWrapper.Instance.PeerLeftEvent += ParticipantLeft;

        await UniTask.WaitUntil(() => session.GetLocalParticipantEntity() != null, cancellationToken: _cancellationTokenSource.Token);
        _timerHandler.Setup(session.GetLocalParticipantEntity().Id, _isHost);
        _timerHandler.OnTimerUpdate += TimerUpdate;
    }

    private void Deinitialize()
    {
        BottleSystem.OnPlayerJoined -= ParticipantsJoins;
        _bottleController.CleanUp();
        if (_bottleSystem != null && _session != null)
        {
            _bottleSystem.OnPlayerScore -= PlayerScore;
            try
            {
                _bottleSystem.DeleteScoreComponent(_myFish.AukiEntity);
                _bottleSystem.DeletePlayerData(_myFish.AukiEntity.Id);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Error trying to clean up score and player component: {e.Message}");
            }
        }
        _bottleSystem = null;
        _session = null;

        _gameStateHandler.CleanUp();
        _gameStateHandler.OnStateUpdate -= OnStateUpdated;
        if (_isHost)
            _timerHandler.CancelTimer();
        _timerHandler.Cleanup();
        _timerHandler.OnTimerUpdate -= TimerUpdate;
        ARAquariumUIController.OnLeaveGame -= LeaveGame;
    }

    private void CalculateScores()
    {
        var scoreUpdate = new Dictionary<uint, int>(PlayerScores);
        foreach (var item in PlayerScores)
        {
            if (_playersLeft.ContainsKey(item.Key))
                scoreUpdate[item.Key] = item.Value / (_playersLeft[item.Key] ? 2 : 1);
        }

        PlayerScores = scoreUpdate;

        var summary = new ScoreSummary()
        {
            playerScores = PlayerScores,
            playersLeftEarly = _playersLeft
        };

        Logger.Log("Show Score Summary");
        _showScores?.Invoke(summary);
    }

    private void LogDuration()
    {
        var userId = GameController.PlayerData.appleUserId;
        _sessionDuration = (Time.time - _startTime) / 60;
        Analytices.Instance.DomainSessionDuration(_domainName, _domainId, userId, _sessionDuration);
    }

    #region CALLBACKS
    private void HostAppointedEvent(bool beenSelected)
    {
        if (!beenSelected) return;

        _isHost = beenSelected;
        _gameStateHandler.ChangeHost(_isHost);
        _timerHandler.ChangeHost(_isHost);

        Logger.Log($"[Host] Host change in game, continuing game state: {_gameState}");

        switch (_gameState)
        {
            case ARGameState.GameInSession:
                _timerHandler.RunTimer(_timer, EndGame);
                Logger.Log($"[Host] continuing timer from: {_timer}");
                break;
            case ARGameState.Waiting:
                _timerHandler.RunTimer(_timer, CommenceGame);
                Logger.Log($"[Host] continuing timer from: {_timer}");
                break;
        }
    }

    private async void ParticipantsJoins(uint participantId, PlayerData data)
    {
        if (_playersInSession.Contains(participantId)) return;

        _playersInSession.Add(participantId);

        if (participantId == _session.ParticipantId)
        {
            _onUpdateWaitMessage?.Invoke(string.Format(Localization.Get(KeyWaitForParticipant), 4 - _playersInSession.Count), -1);
            return;
        }

        if (_isHost && _playersInSession.Count == _maxPlayers)
        {
            _timerHandler.CancelTimer();
            CommenceGame();
            return;
        }

        if (_gameState == ARGameState.GameInSession) return;

        var waitMessage = string.Format(Localization.Get(KeyParticipantJoined), data.appleName);
        _onUpdateWaitMessage?.Invoke(waitMessage, 3);
        await UniTask.WaitForSeconds(3, cancellationToken: _cancellationTokenSource.Token);
        if (_gameState == ARGameState.GameInSession) return;
        
        waitMessage = string.Format(Localization.Get(KeyWaitForParticipant), 4 - _session.GetParticipantCount());
        _onUpdateWaitMessage?.Invoke(waitMessage, -1);
    }

    private void ParticipantLeft(uint playerId)
    {
        if (_gameState != ARGameState.GameInSession) return;
        _playersLeft.Add(playerId, true);
        if (_playersInSession.Contains(playerId)) _playersInSession.Remove(playerId);
    }

    private void PlayerScore(uint playerId, int score)
    {
        var combinedScore = _bottleCollectedPreviously + score;
        PlayerScores[playerId] = score;
        OnPlayerScore?.Invoke(playerId, score);
    }

    private void TimerUpdate(int timer)
    {
        _timer = timer;
        OnTimerUpdate?.Invoke(timer);
    }

    private void SendUpdateState(ARGameState state)
    {
        _gameStateHandler.SendStateUpdate(_session.GetLocalParticipantEntity().Id, state);
    }

    private void OnStateUpdated(ARGameState state)
    {
        _gameState = state;
        switch (state)
        {
            case ARGameState.GameInSession:
                InitializeScore();
                _bottleController.Setup(_session.GetParticipantCount() + 1, _bottleSystem, _session, _isHost);
                OnStateChange?.Invoke(state);
                break;
            case ARGameState.GameEnd:
                AukiSdkWrapper.Instance.LeftEvent -= LeftEvent;
                LogDuration();
                CalculateScores();
                Deinitialize();
                OnStateChange?.Invoke(state);
                if (!GameSave.MultiPlayerModeLimitReached)
                {
                    OnAnimateCelebration?.Invoke();
                    ServerTimeHelper.UpdateLastSessionDateTime();
                }
                if (NetworkMatchHandler.AddMatch())
                {
                    OnLimitAchieved?.Invoke();
                }
                break;
            default:
                OnStateChange?.Invoke(state);
                break;
        }
    }
    #endregion
}