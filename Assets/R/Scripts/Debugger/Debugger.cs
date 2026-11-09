using DG.Tweening;
using UnityEngine;
using IngameDebugConsole;
using Network;
using ProGamer;

public class Debugger : ProGamer.Singleton<Debugger>
{
    [SerializeField] private DOTweenAnimation signin;
    public float HandTrackingChargingTime { get; private set; } = -1;
    public int EventIndex { get; set; } = -1;
    public int EventImageIndex { get; set; } = -1;
    public bool AllFishDeactived { get; set; } = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyUp(KeyCode.H))
            {
                Transform camTransform = Camera.main.transform;
                var targetPosition = camTransform.position + camTransform.forward * 1f;
                StartCoroutine(ARMetaverseManager.Instance.CreateMyMagicObjectAndSync(targetPosition, MagicObjectTypeEnum.Heart));
            }

            if (Input.GetKeyUp(KeyCode.T))
            {
                Transform camTransform = Camera.main.transform;
                var targetPosition = camTransform.position + camTransform.forward * 1f;
                StartCoroutine(ARMetaverseManager.Instance.CreateMyMagicObjectAndSync(targetPosition, MagicObjectTypeEnum.Torus));
            }

            if (Input.GetKeyUp(KeyCode.B))
            {
                Transform camTransform = Camera.main.transform;
                var targetPosition = camTransform.position + camTransform.forward * 1f;
                StartCoroutine(ARMetaverseManager.Instance.CreateMyMagicObjectAndSync(targetPosition, MagicObjectTypeEnum.Bowl));
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                Debug.Log(Localization.language);
            }
        }
    }

    #region Server Apis

    [ConsoleMethod("gterm", "Accept T&C.")]
    public static void GetTerms()
    {
        ServerCenter.Instance.GetTerms(_ =>
        {
            Debug.Log("111111 Test Server API - GetTerms");
        }, null);
    }

    [ConsoleMethod("aterm", "Accept T&C.")]
    public static void AcceptTerms()
    {
        ServerCenter.Instance.AcceptTerms(_ =>
        {
            Debug.Log("111111 Test Server API - AcceptTerms");
        }, null);
    }

    [ConsoleMethod("login", "Sign in with Apple to game server.")]
    public static void LoginServer()
    {
        ServerCenter.Instance.SignInWithApple(_ =>
        {
            Debug.Log("111111 Test Server API - LoginServer");
        }, null, null);
    }

    [ConsoleMethod("user", "get user info.")]
    public static void GetUserInfo()
    {
        ServerCenter.Instance.UpdateUserInfo(_ =>
        {
            Debug.Log("111111 Test Server API - GetUserInfo");
        });
    }

    [ConsoleMethod("ver", "get latest app version.")]
    public static void GetLatestVersion()
    {
        ServerCenter.Instance.UpdateLatestVersion(_ =>
        {
            Debug.Log("111111 Test Server API - GetLatestVersion");
        });
    }

    [ConsoleMethod("events", "get events from server.")]
    public static void GetEvents()
    {
        ServerCenter.Instance.UpdateEvents(() =>
        {
            Debug.Log("111111 Test Server API - GetEvents");
        });
    }

    [ConsoleMethod("nft", "get nft list.")]
    public static void GetNfts()
    {
        ServerCenter.Instance.UpdateNftsInfo(_ =>
        {
            Debug.Log("111111 Test Server API - GetNfts");
        }, null);
    }

    [ConsoleMethod("arts", "get artwork list.")]
    public static void GetArtworks()
    {
        ServerCenter.Instance.UpdateArtworkList(_ =>
        {
            Debug.Log("111111 Test Server API - GetArtworks");
        }, null);
    }

    [ConsoleMethod("uploadart", "post artwork info.")]
    public static void UploadArtwork()
    {
        ServerCenter.Instance.UploadArtwork(1, "fish", "sampleArtworkUrl3",
            () =>
            {
                Debug.Log("111111 Test Server API - UploadArtwork");
            }, null);
    }

    [ConsoleMethod("files", "get artwork file list.")]
    public static void GetArtworkFiles()
    {
        ServerCenter.Instance.UpdateArtworkFileList(_ =>
        {
            Debug.Log("111111 Test Server API - GetArtworkFiles");
        }, null);
    }

    [ConsoleMethod("uploadfile", "upload artwork file.")]
    public static void UploadArtworkFile()
    {
        ServerCenter.Instance.UploadArtworkFile(ImageDataManager.Instance.GetAquariumSnapshotData(GameSetting.HatcheryAquariumId), _ =>
        {
            Debug.Log("111111 Test Server API - UploadArtworkFile");
        }, null);
    }

    #endregion

    #region Saved Data

    [ConsoleMethod("fakefish", "clear archived fishes and copy all active fishes to archived fishes")]
    public static void UseDebugFishes()
    {
        ImageDataManager.Instance.UseDebugFishes(true);
    }

    [ConsoleMethod("resetiap", "clear saved iap items.")]
    public static void ResetIap()
    {
        Debug.Log($"[Debugger] clear saved iap items.");
        GameSave.PurchaseRecord = "";
        DataCenter.Instance.mPurchaseStatus = null;
    }

    [ConsoleMethod("unlockiap", "unlock all iap items.")]
    public static void UnlockIap()
    {
        Debug.Log($"[Debugger] unlock all iap items.");
        GameSave.PurchaseRecord = "{\"packageBand01\":true,\"packageBand04\":true,\"packageBand05\":true}";
        DataCenter.Instance.mPurchaseStatus = null;
    }

    [ConsoleMethod("cleardefault", "clear default fish.")]
    public static void ClearDefault()
    {
        Debug.Log($"[Debugger] clear default fish.");
        GameSave.DefaultFishAdded = false;
    }

    #endregion

    #region AR

    [ConsoleMethod("heart", "create new heart.")]
    public static void Heart()
    {
        Transform camTransform = Camera.main.transform;
        var targetPosition = camTransform.position + camTransform.forward * 1f;
        Instance.StartCoroutine(ARMetaverseManager.Instance.CreateMyMagicObjectAndSync(targetPosition, MagicObjectTypeEnum.Heart));
    }

    #endregion

    #region Environment

    [ConsoleMethod("netmock", "set network to mock server.")]
    public static void NetMock()
    {
        Debug.Log($"[Debugger] set network environment to mock server.");
        NetworkSettings.Instance.SavedEnvironment = (int)NetworkEnvironmentEnum.Mock;
    }

    [ConsoleMethod("nettest", "set network to develop server.")]
    public static void NetTest()
    {
        Debug.Log($"[Debugger] set network environment to test server.");
        NetworkSettings.Instance.SavedEnvironment = (int)NetworkEnvironmentEnum.Develop;
    }

    [ConsoleMethod("usenft", "use nft or artwork list in metaverse.")]
    public static void UseNftInMetaverse(int value)
    {
        string name = value > 0 ? "nft" : "artwork";
        Debug.Log($"[Debugger] use {name} list in metaverse.");
        ServerCenter.Instance.UseArtworkInMetaverse = value == 0;
    }

    #endregion

    #region Other

    [ConsoleMethod("gescharging", "set the charging time of hand gesture tracking.")]
    public static void SetGestureChargingTime(float time)
    {
        Debug.Log($"[Debugger] set hand gesture charging time to {time}");
        Instance.HandTrackingChargingTime = time;
    }

    [ConsoleMethod("fish", "set fish image file of event for testing.")]
    public static void SetEventImageFile(int index)
    {
        Debug.Log($"[Debugger] set event image file index to {index}");
        Instance.EventImageIndex = index;
    }

    [ConsoleMethod("oldver", "set expired version.")]
    public static void SetOldVersion()
    {
        Debug.Log("[Debugger] set version to be expired.");
        GameSetting.Instance.DevVersionId = 0;
    }

    [ConsoleMethod("newver", "set version to be up to date.")]
    public static void SetNewVersion()
    {
        Debug.Log("[Debugger] set version number to 99.");
        GameSetting.Instance.DevVersionId = 99;
    }

    /// <summary>
    /// index:
    /// - 0: coming soon
    /// - 1: start in 7 days
    /// - 2: start soon (in 1 day)
    /// - 3: active (more than 1 day left)
    /// - 4: end soon (in 1 day)
    /// - 5: ended
    /// </summary>
    /// <param name="index"></param>
    [ConsoleMethod("event", "switch event.")]
    public static void SetEvent(int index)
    {
        Debug.Log($"[Debugger] switch event to {index}.");
        Instance.EventIndex = index;
    }
    
    /// usage:
    /// command: deactiveall
    /// command: event [5]
    /// all current fishes should be moved to archived. 
    [ConsoleMethod("deactiveall", "set all fishe events to be ended, test archive.")]
    public static void DeactiveAllFishes()
    {
        Debug.Log("[Debugger] set all finishes finished.");
        Instance.AllFishDeactived = true;
    }
    
    /// usage:
    /// command: deactiveall
    /// command: event [5]
    /// all current fishes should be moved to archived. 
    [ConsoleMethod("updateMock", "update mock files in runtime.")]
    public static void UpdateMock()
    {
        Debug.Log("[Debugger] update mock files in runtime.");
        MockServer.Instance.ReloadAllResponseData();
    }

    #endregion
}