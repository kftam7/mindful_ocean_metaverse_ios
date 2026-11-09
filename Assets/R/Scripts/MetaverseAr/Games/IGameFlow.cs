using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using UnityEngine;

/// <summary>
/// Game flow interface, should handle:
/// - States
/// - Matchmaking (single player can just add delay/countdown)
/// - Game session (Timer, Bottle Spawning)
/// - Game finished
/// </summary>
public interface IGameFlow
{
    Dictionary<uint, int> PlayerScores { get; }
    void StartGame(Session session, string domainId, string domainName);
    event Action<ARGameState> OnStateChange;
    event Action<int> OnTimerUpdate;
    event Action<uint, int> OnPlayerScore;
    event Action OnLimitAchieved;
    event Action OnAnimateCelebration;
    event Action<Queue<TipQueue>> OnShowTip;

    Queue<TipQueue> InitializeTip(string path, string[] names)
    {
        Queue<TipQueue> tipQueues = new();
        for (int i = 0; i < names.Length; i++)
        {
            var resPath = $"{path}/{names[i]}";
            var tipData = Resources.Load<TipQueue>(resPath);
            tipData.Initialize();
            tipQueues.Enqueue(tipData);
        }
        return tipQueues;
    }
}