using System;
using Game.Models;
using Models;
using Newtonsoft.Json;
using Romi.Communications;
using UnityEngine;

public class BottlesCollection : MonoBehaviour
{
    [field: SerializeField] public GameConfig config { get; private set; }
    private const string Get = "GET";
    private const string Post = "POST";
    private const string Patch = "PATCH";
    private const string Delete = "DELETE";

    private const string ApiUrl = "http://16.162.241.30:8090/";
    private const string ApiEndPoint = "api/collections/bottles/records";

    private IServerCommunication _serverCommunication;
    private bool _bottleEntryExist;
    private string _entryId;

    public static int SinglePlayerBottlesCollected { get; private set; }
    public static int TotalBottlesCollected { get; private set; }

    private void Awake()
    {
        _serverCommunication = new APICommunication(ApiUrl);
    }

    void Start()
    {
        ConfigController.OnConfigFetched += ConfigFetched;
        GameController.OnFinishGame += SubmitBottles;
        ServerTimeHelper.OnServerTimeFetched += InitializeDailyTracking;
    }

    void OnDestroy()
    {
        ConfigController.OnConfigFetched -= ConfigFetched;
        GameController.OnFinishGame -= SubmitBottles;
        ServerTimeHelper.OnServerTimeFetched -= InitializeDailyTracking;
    }

    private void InitializeDailyTracking(DateTime startDateTime)
    {
        // Check if a new day has started
        if (ServerTimeHelper.HasDayPassed())
        {
            Logger.Log($"[{name}] Day has passed, resetting values");
            // Reset daily counter if it's a new day
            SinglePlayerBottlesCollected = 0;
            GameSave.SinglePlayerBottlesCollectedToday = 0;
            GameSave.SinglePlayerModeLimitReached = false;
        }
        else
        {
            // Load previous day's data
            SinglePlayerBottlesCollected = GameSave.SinglePlayerBottlesCollectedToday;
        }
        Logger.Log($"[{name}] Single Player Bottle Collection Today: {SinglePlayerBottlesCollected}, Total with Multi: {TotalBottlesCollected}");
    }

    private void ConfigFetched(GameConfig config)
    {
        this.config = config;
        FetchBottleCollectedAmount(GameController.PlayerData.appleEmail);
    }

    private void SubmitBottles(bool singlePlayerMode, int score)
    {
        Logger.Log($"[Collection] Submitting Bottles to Backend, Is SinglePLayer: {singlePlayerMode}, score to submit: {score}");
        if (singlePlayerMode)
            SubmitBottlesSinglePlayer(score);
        else
            SubmitBottlesMultiPlayer(score);
    }


    private void SubmitBottlesSinglePlayer(int score)
    {
        if (GameSave.SinglePlayerModeLimitReached) return;

        // Calculate how many bottles we can actually add today
        int availableCapacity = GetRemainingDailyCapacity();
        int bottlesToAdd = Mathf.Min(score, availableCapacity);

        if (bottlesToAdd <= 0)
        {
            Logger.Log("[Collection] Daily bottle limit reached. No bottles added.");
            return;
        }

        // Update counters
        SinglePlayerBottlesCollected += bottlesToAdd;
        TotalBottlesCollected += bottlesToAdd;

        GameSave.SinglePlayerBottlesCollectedToday = SinglePlayerBottlesCollected;
        // GameSave.TotalBottlesCollectedToday = TotalBottlesCollected;

        Logger.Log($"[Collection] Added {bottlesToAdd} bottles today. Total today: {SinglePlayerBottlesCollected}/{config.MaxBottlePerDay}");

        SubmitRecords();
    }

    private void SubmitBottlesMultiPlayer(int score)
    {
        if (GameSave.MultiPlayerModeLimitReached) return;

        // Save daily progress
        TotalBottlesCollected += score;
        // GameSave.TotalBottlesCollectedToday = TotalBottlesCollected;

        Logger.Log($"[Collection] Added {score} bottles today. Total today collection combined: {TotalBottlesCollected}");

        SubmitRecords();
    }

    private async void FetchBottleCollectedAmount(string email)
    {
        var endPointWithFilter = $"{ApiEndPoint}?filter=(email='{email}')";
        var result = await _serverCommunication.Send(endPointWithFilter, Get, null, null);

        if (result.status == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var bottleRecords = JsonConvert.DeserializeObject<CollectionResponse<BottleCollectResponse>>(result.content);

            _bottleEntryExist = bottleRecords != null && bottleRecords.Items.Count > 0;

            if (_bottleEntryExist)
            {
                var bottleEntry = bottleRecords.Items[0];

                Logger.Log($"[Collection] Collection record found, ID: {bottleEntry.Id}");
                TotalBottlesCollected = bottleEntry.Collected;
                _entryId = bottleEntry.Id;
            }
            else
            {
                SubmitRecords();
            }
        }
        else
        {
            var error = $"[{name}]{result.status} : {result.content}";
            Logger.LogError(error);
        }
    }

    public async void SubmitRecords()
    {
        var method = _bottleEntryExist ? Patch : Post;
        var body = new BottleCollectedPayload()
        {
            Username = GameController.PlayerData.appleName,
            Email = GameController.PlayerData.appleEmail,
            Collected = TotalBottlesCollected
        };
        var endPoint = _bottleEntryExist ? $"{ApiEndPoint}/{_entryId}" : ApiEndPoint;
        var result = await _serverCommunication.Send(endPoint, method, JsonConvert.SerializeObject(body), null);

        Logger.Log($"[Collection] Updated Bottle: {result.status}");

        var response = JsonConvert.DeserializeObject<BottleCollectResponse>(result.content);
        _entryId = response.Id;
    }

    // Helper method to check remaining daily capacity
    public int GetRemainingDailyCapacity()
    {
        return Mathf.Max(0, config.MaxBottlePerDay - SinglePlayerBottlesCollected);
    }
}