using System;
using Cysharp.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using Romi.Communications;
using UnityEngine;

public class ServerTimeHelper : MonoBehaviour
{
    public static ServerTimeHelper Instance { get; private set; }
    [SerializeField] private bool useDeviceTime;

    [Header("For Testing")]
    [SerializeField] private int offsetDays;
    private string _timeZoneId = "Hongkong";
    private const string ApiUrl = "https://timeapi.io/api/time/current/zone";
    private const string EndPoint = "?timeZone=";

    private IServerCommunication _serverCommunication;
    private int _tries = 3;
    private static float _fetchSuccessTime;
    private static int _offsetDays;

    private static DateTime _sessionStartServerTime;

    public static DateTime SessionStartServerTime => _sessionStartServerTime;

    public static event Action<DateTime> OnServerTimeFetched;

    void Awake()
    {
        Instance = this;
        _serverCommunication = new APICommunication(ApiUrl);
        _timeZoneId = TimeZoneInfo.Local.Id;
        _offsetDays = offsetDays;    
    }

    private void OnValidate()
    {
        _offsetDays = offsetDays;    
    }

    async void Start()
    {
        if (useDeviceTime)
        {
            _sessionStartServerTime = DateTime.Now;
            OnServerTimeFetched?.Invoke(_sessionStartServerTime);
            return;
        }

        var dateTimeResult = await FetchServerTime();
        _sessionStartServerTime = dateTimeResult;
        OnServerTimeFetched?.Invoke(_sessionStartServerTime);
    }

    void OnApplicationQuit()
    {
        UpdateLastSessionDateTime();
    }

    private async UniTask<DateTime> FetchServerTime()
    {
        var tries = 0;

        while (tries < _tries)
        {
            var result = await _serverCommunication.Send($"{EndPoint}{_timeZoneId}", "GET", string.Empty, string.Empty);

            if (result.status == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var serverTimeData = JsonConvert.DeserializeObject<ServerTimeEntry>(result.content);
                Logger.Log($"[Server Time]:{serverTimeData.DateTime}, {serverTimeData.TimeZone}");
                _fetchSuccessTime = Time.time;
                return serverTimeData.DateTime;
            }
            else
            {
                tries++;
                await UniTask.WaitForSeconds(0.25f);
            }
        }

        return DateTime.UtcNow;
    }

    public static DateTime GetLatestTime()
    {
        return _sessionStartServerTime.AddDays(_offsetDays).AddSeconds(Time.time - _fetchSuccessTime);
    }

    public static bool HasDayPassed()
    {
        var latestDate = GetLatestTime().Date;
        return latestDate != GameSave.LastSessionDateTime.Date;
    }

    public static void UpdateLastSessionDateTime()
    {
        GameSave.LastSessionDateTime = GetLatestTime();
    }
}