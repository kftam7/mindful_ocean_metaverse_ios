using System;
using UnityEngine;

public class NetworkMatchHandler : MonoBehaviour
{
    private static int _matchLimit;

    void Start()
    {
        ConfigController.OnConfigFetched += ConfigFetched;
        ServerTimeHelper.OnServerTimeFetched += ServerTimeFetched;
    }

    void OnDestroy()
    {
        ConfigController.OnConfigFetched -= ConfigFetched;
        ServerTimeHelper.OnServerTimeFetched -= ServerTimeFetched;
    }

    private void ConfigFetched(GameConfig config)
    {
        _matchLimit = config.MaxMatchPerDay;

        if (GameSave.MultiPlayerModeMatchCount < _matchLimit)
            GameSave.MultiPlayerModeLimitReached = false;
    }

    private void ServerTimeFetched(DateTime dateTime)
    {
        if (ServerTimeHelper.HasDayPassed())
        {
            GameSave.MultiPlayerModeLimitReached = false;
            GameSave.MultiPlayerModeMatchCount = 0;
        }
    }

    /// <summary>
    /// Count multiplayer match to user persistent storage
    /// </summary>
    /// <returns>true if limit achieved</returns>
    public static bool AddMatch()
    {
        if (!GameSave.MultiPlayerModeLimitReached)
        {
            var value = GameSave.MultiPlayerModeMatchCount;
            GameSave.MultiPlayerModeMatchCount = value + 1;
        }

        Logger.Log($"Multiplayer Match Total So far: {GameSave.MultiPlayerModeMatchCount}");

        if (GameSave.MultiPlayerModeMatchCount == _matchLimit)
        {
            GameSave.MultiPlayerModeLimitReached = true;
        }

        return GameSave.MultiPlayerModeLimitReached;
    }
}