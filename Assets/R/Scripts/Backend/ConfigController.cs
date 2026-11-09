using System;
using System.Web;
using Models;
using Newtonsoft.Json;
using Romi.Communications;
using UnityEngine;

public class ConfigController : MonoBehaviour
{
    [field:SerializeField] public GameConfig config { get; private set; }
    private const string Get = "GET";
    private const string ApiUrl = "http://16.162.241.30:8090/";
    private const string ApiEndPoint = "api/collections/configs/records";
    private const string ConfigName = "default";

    private IServerCommunication _serverCommunication;

    public static event Action<GameConfig> OnConfigFetched;

    private void Awake()
    {
        _serverCommunication = new APICommunication(ApiUrl);
    }

    void Start()
    {
        FetchConfig();
    }

    private async void FetchConfig()
    {
        var result = await _serverCommunication.Send(ApiEndPoint, Get, null, null);

        if (result.status == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var configs = JsonConvert.DeserializeObject<CollectionResponse<ConfigEntry<GameConfig>>>(result.content);

            if (configs != null && configs.Items.Count > 0)
            {
                var configEntry = configs.Items.Find(x => x.ConfigName.Equals(ConfigName, StringComparison.CurrentCultureIgnoreCase));

                config = configEntry.Config;
            }
        }

        OnConfigFetched?.Invoke(config);
    }
}