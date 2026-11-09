using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.ConjureKit.Vikja;
using Auki.Util;
using Firebase.Analytics;
using ProGamer;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARMetaverseManager : SceneInstance<ARMetaverseManager>
{
    #region Serialize Fields

    [SerializeField] private GameObject seaGO;
    [SerializeField] private GameObject waterLightGO;
    [SerializeField] private GameObject bubblesGO;
    [SerializeField] private GameObject fadeInGO;
    [SerializeField] private GameObject fadeOutGO;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject torusPrefab;
    [SerializeField] private GameObject bowlPrefab;
    [SerializeField] private Camera arCamera;
    [SerializeField] private int maxMagicObjectNumber = 999;
    [SerializeField] private bool useDebugQr;

    #endregion

    private ARSessionOrigin sessionOrigin;
    private IEnumerator underWaterSoundThread;
    private FishTank fishTank;
    private Dictionary<uint, GameObject> entityObjectDic = new(); // auki entity id -> game object
    private List<FishEntity> peerFishes = new();
    private GestureHintPopup gestureHintPopup;
    private bool scanSuccess;
    private bool justJoined;

    private int MyMagicObjectCount => MagicObjects.Count(magicObject => magicObject.Owned);

    public FishEntity MyFish { get; private set; }
    public const string MOMQrShortId = "GL2G5D65Q8M"; 
    public const string MOMQrShortId_debug = "YI3753E78HX";
    public const string LittleAdventureId_Debug = "ac811098-d14f-4938-8b96-c89a5c6d490d";
    public const string LittleAdventureId = "4d436ae0-0ecc-4ba5-83aa-3a11909d400b";

    private string UserName
    {
        get
        {
#if !UNITY_EDITOR
            string firstName = ServerCenter.Instance.AppleFirstName;
            string lastName = ServerCenter.Instance.AppleLastName;
            return string.IsNullOrEmpty(firstName)
                ? string.IsNullOrEmpty(lastName)
                    ? Localization.Get("txt_magic_friend")
                    : CutString(lastName, 18, "...")
                : CutString(firstName, 18, "...");
#else
            return $"{GameController.PlayerData.appleName ?? string.Empty}";
#endif
        }
    }


    #region Public Fields

    public List<MagicObject> MagicObjects { get; } = new();

    #endregion

    #region Private Fields

    private ARSessionOrigin SessionOrigin
    {
        get
        {
            if (sessionOrigin == null)
            {
                sessionOrigin = GetComponent<ARSessionOrigin>();
            }

            return sessionOrigin;
        }
    }

    #endregion

    #region Unity Methods

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (SessionOrigin != null && SessionOrigin.trackablesParent != null)
        {
            SessionOrigin.trackablesParent.gameObject.SetActive(false);
        }

        //disable blue layout and water filter
        EnableOceanEffect(false);
        InitEnvironment();
        HandGestureTracker.Instance.GestureTriggerEvent += OnGestureTrigger;
        HandGestureTracker.Instance.GestureChargingEvent += OnGestureCharging;
        HandGestureTracker.Instance.GestureLostChargingEvent += OnGestureLostCharging;
        AukiSdkWrapper.Instance.JoinedEvent += OnJoined;
        AukiSdkWrapper.Instance.LeftEvent += OnLeft;
        AukiSdkWrapper.Instance.EntityDeletedEvent += OnEntityDeleted;
        AukiSdkWrapper.Instance.EntityUpdatePoseEvent += OnEntityUpdatePose;
        AukiSdkWrapper.Instance.EntityActionEvent += OnEntityAction;
        AukiSdkWrapper.Instance.PeerJoinedEvent += OnParticipantJoined;
        AukiSdkWrapper.Instance.LighthouseTrackedEvent += OnLighthouseTracked;
        AukiSdkWrapper.Instance.OnHostingDomainSession += OnHostingDomainSession;
        AukiSdkWrapper.Instance.OnJoinedDomainSession += OnJoinedDomainSession;
        AukiSdkWrapper.Instance.OnDomainAssetsAvailable += OnDomainAssetsAvailable;
    }

    private void OnDestroy()
    {
        HandGestureTracker.Instance.GestureTriggerEvent -= OnGestureTrigger;
        HandGestureTracker.Instance.GestureChargingEvent -= OnGestureCharging;
        HandGestureTracker.Instance.GestureLostChargingEvent -= OnGestureLostCharging;
        AukiSdkWrapper.Instance.JoinedEvent -= OnJoined;
        AukiSdkWrapper.Instance.LeftEvent -= OnLeft;
        AukiSdkWrapper.Instance.EntityDeletedEvent -= OnEntityDeleted;
        AukiSdkWrapper.Instance.EntityUpdatePoseEvent -= OnEntityUpdatePose;
        AukiSdkWrapper.Instance.EntityActionEvent -= OnEntityAction;
        AukiSdkWrapper.Instance.PeerJoinedEvent -= OnParticipantJoined;
        AukiSdkWrapper.Instance.LighthouseTrackedEvent -= OnLighthouseTracked;
        AukiSdkWrapper.Instance.OnHostingDomainSession -= OnHostingDomainSession;
        AukiSdkWrapper.Instance.OnJoinedDomainSession -= OnJoinedDomainSession;
        AukiSdkWrapper.Instance.OnDomainAssetsAvailable -= OnDomainAssetsAvailable;
    }

    #endregion

    #region Environment

    public void EnableOceanEffect(bool value)
    {
        bool enable = value && SystemExtension.isFastCPU();
        arCamera.GetComponent<CameraFilterPack_Distortion_Dream2>().enabled = enable;
        arCamera.GetComponent<CameraFilterPack_Color_RGB>().enabled = enable;
    }

    #endregion

    #region Public Methods

    public void Disconnect()
    {
        DeleteMyFishFromServer();
        DeleteMyMagicObjectsFromServer();
        AukiSdkWrapper.Instance.Disconnect();
    }

    #endregion

    #region Create Magic Object

    public IEnumerator CreateMyMagicObjectAndSync(Vector3 position, MagicObjectTypeEnum type)
    {
        Logger.Log($"[{GetType().Name}] CreateMyMagicObjectAndSync_1 {type}");
        MagicObject magicObject = CreateMagicObject(position, type, UserName, true);
        SoundManager.Instance.Play("blop");

        while (!AukiSdkWrapper.Instance.HasJoined)
        {
            yield return null;
        }

        if (magicObject != null && !magicObject.IsDisappearing)
        {
            AukiSdkWrapper.Instance.AddMagicEntity(UserName, new Pose(position, magicObject.transform.rotation), type, true, entity =>
            {
                magicObject.AukiEntity = entity;
                entityObjectDic.Add(entity.Id, magicObject.gameObject);
                Logger.Log($"[{GetType().Name}] CreateMyMagicObjectAndSync_2 {entity.Id}");
            });
        }
    }

    private MagicObject CreateMagicObject(Vector3 position, MagicObjectTypeEnum type, string ownerName, bool owned = false)
    {
        Logger.Log($"[{GetType().Name}] CreateMagicObject {type}");
        MagicObject magicObject;
        if (type == MagicObjectTypeEnum.Heart)
        {
            magicObject = Instantiate(heartPrefab).GetComponent<MagicObject>();
        }
        else if (type == MagicObjectTypeEnum.Torus)
        {
            magicObject = Instantiate(torusPrefab).GetComponent<MagicObject>();
        }
        else
        {
            magicObject = Instantiate(bowlPrefab).GetComponent<MagicObject>();
        }

        magicObject.Owned = owned;
        MagicObjects.Add(magicObject);
        magicObject.transform.position = position;
        PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);

        var hintText = Localization.Get(owned
            ? "txt_magic_self_create_hint"
            : "txt_magic_friend_create_hint");

        hintText = hintText.Replace("{0}", ownerName);
        hintText = type switch
        {
            MagicObjectTypeEnum.Heart => hintText.Replace("{1}", Localization.Get("txt_magic_heart")),
            MagicObjectTypeEnum.Bowl => hintText.Replace("{1}", Localization.Get("txt_magic_spin")),
            MagicObjectTypeEnum.Torus => hintText.Replace("{1}", Localization.Get("txt_magic_hula")),
            _ => hintText.Replace("{1}", Localization.Get("txt_magic_heart"))
        };

        var popup = PopupManager.Instance.ActivePopup as HintPopup;
        popup.SetTextTop(hintText);
        popup.SetDelayClose(2);

        return magicObject;
    }

    private void DeleteAllMagicObjects()
    {
        Logger.Log($"[{GetType().Name}] DeleteAllMagicObjects");
        for (var i = MagicObjects.Count - 1; i >= 0; i--)
        {
            MagicObject magicObject = MagicObjects[i];
            if (magicObject != null)
            {
                Destroy(magicObject.gameObject); // will be removed from AllMagicObjects in OnDestroy()}
            }
        }
    }

    public void DeleteMyMagicObjectsFromServer()
    {
        Logger.Log($"[{GetType().Name}] DeleteMyMagicObjectsFromServer_1");
        Logger.Log($"Count: {MagicObjects.Count}");
        for (var i = MagicObjects.Count - 1; i >= 0; i--)
        {
            MagicObject magicObject = MagicObjects[i];
            if (magicObject != null && magicObject.Owned && magicObject.AukiEntity != null)
            {
                Logger.Log($"[{GetType().Name}] DeleteMyMagicObjectsFromServer_2 {magicObject.AukiEntity.Id}");
                entityObjectDic.Remove(magicObject.AukiEntity.Id);
                AukiSdkWrapper.Instance.DeleteEntity(magicObject.AukiEntity.Id, id => { });
                magicObject.AukiEntity = null;
            }
        }
    }

    // Used by MagicObject.OnDestroy()
    public void RemoveMagicObjectFromList(MagicObject magicObject)
    {
        Logger.Log($"[{GetType().Name}] RemoveMagicObjectFromList_1");
        MagicObjects.Remove(magicObject);

        if (magicObject != null && magicObject.AukiEntity != null) // may be destroyed before join a session
        {
            Logger.Log($"[{GetType().Name}] RemoveMagicObjectFromList_2 {magicObject.AukiEntity.Id}");
            entityObjectDic.Remove(magicObject.AukiEntity.Id);
        }
    }

    #region Create Fish

    private FishEntity CreateFish(string ownerName, string imageUrl, Vector3 position, bool owned = false)
    {
        Logger.Log($"[{GetType().Name}] CreateFish owned: {owned} url: {imageUrl}");
        FishEntity fishEntity = fishTank.CreateFish(imageUrl, 3, new Vector3(0, 1.3f, 0), 1, position);
        fishEntity.OwnerName = ownerName;
        fishEntity.ImageUrl = imageUrl;
        fishEntity.Owned = owned;
        if (owned)
            MyFish = fishEntity;
        else
            peerFishes.Add(fishEntity);
        return fishEntity;
    }

    public static event Action<FishEntity> OnFishAddedToServer;

    private void AddMyFishToServer()
    {
        Logger.Log($"[{GetType().Name}] AddMyFishToServer_1");
        if (MyFish == null) return;
        var fishTransform = MyFish.transform;
        AukiSdkWrapper.Instance.AddFishEntity(new Pose(fishTransform.position, fishTransform.rotation),
            false, MyFish.OwnerName, MyFish.ImageUrl, entity =>
            {
                Logger.Log($"[{GetType().Name}] AddMyFishToServer_2 {entity.Id}");
                MyFish.AukiEntity = entity;
                entityObjectDic.Add(entity.Id, MyFish.gameObject);
                OnFishAddedToServer?.Invoke(MyFish);
            });
    }

    public void DeleteMyFishFromServer()
    {
        Logger.Log($"[{GetType().Name}] DeleteMyFishFromServer_1");
        if (MyFish != null && MyFish.AukiEntity != null)
        {
            entityObjectDic.Remove(MyFish.AukiEntity.Id);
            Logger.Log($"[{GetType().Name}] DeleteMyFishFromServer_2 {MyFish.AukiEntity.Id}");
            AukiSdkWrapper.Instance.DeleteEntity(MyFish.AukiEntity.Id, id =>
            {
                Logger.Log($"[{GetType().Name}] DeleteMyFishFromServer_3 success {id}");
            });
            MyFish.AukiEntity = null;
        }
    }

    private void DestroyAllPeerFishes()
    {
        Logger.Log($"[{GetType().Name}] DestroyPeerFishes");
        foreach (FishEntity fishEntity in peerFishes)
        {
            if (fishEntity == null || fishEntity.AukiEntity == null) continue;
            entityObjectDic.Remove(fishEntity.AukiEntity.Id);
            Destroy(fishEntity.gameObject);
        }

        peerFishes.Clear();
    }

    #endregion

    #endregion

    public void Connect()
    {
        PopupManager.Instance.QuickShowPopup(PopupPrefab.QrCodePopup);
    }

    public void ShowBuiltinDomains()
    {
        AukiSdkWrapper.instance.ShowDomains(GetBuiltInQr());
    }

    public string GetBuiltInQr()
    {
        return useDebugQr ? MOMQrShortId_debug : MOMQrShortId;
    }

    public string GetSinglePlayerDomainId()
    {
        return useDebugQr ? LittleAdventureId_Debug : LittleAdventureId;
    }

    #region Event Callbacks

    private void OnGestureTrigger(HandGestureEnum gesture)
    {
        if (MyMagicObjectCount > maxMagicObjectNumber) return;

        if (gesture == HandGestureEnum.Heart)
        {
            Vector3 position = HandGestureTracker.instance.Joints[JointName.thumbTip].GetWorldPosition(AukiSdkWrapper.Instance.ArCamera, 1f) + new Vector3(0, 0.025f, 0);
            StartCoroutine(CreateMyMagicObjectAndSync(position, MagicObjectTypeEnum.Heart));
        }
        else if (gesture == HandGestureEnum.Torus)
        {
            Vector3 position = HandGestureTracker.instance.Joints[JointName.thumbTip].GetWorldPosition(AukiSdkWrapper.Instance.ArCamera, 1f) + new Vector3(0, 0.035f, 0);
            StartCoroutine(CreateMyMagicObjectAndSync(position, MagicObjectTypeEnum.Torus));
        }
        else if (gesture == HandGestureEnum.Bowl)
        {
            Vector3 thumbPosition = HandGestureTracker.instance.Joints[JointName.thumbMP].GetWorldPosition(AukiSdkWrapper.Instance.ArCamera, 1f);
            Vector3 indexPosition = HandGestureTracker.instance.Joints[JointName.indexMCP].GetWorldPosition(AukiSdkWrapper.Instance.ArCamera, 1f);
            Vector3 position = Vector3.Lerp(thumbPosition, indexPosition, 0.6f) + new Vector3(0, 0.05f, 0);
            StartCoroutine(CreateMyMagicObjectAndSync(position, MagicObjectTypeEnum.Bowl));
        }
    }

    private void OnGestureLostCharging(HandGestureEnum gesture)
    {
        if (gestureHintPopup != null)
        {
            gestureHintPopup.SetBarProgress(0);
            gestureHintPopup.CloseQuietly();
            gestureHintPopup = null;
        }
    }

    private void OnGestureCharging(HandGestureEnum gesture, float chargedTime)
    {
        if (gestureHintPopup == null)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.GestureHintPopup);
            gestureHintPopup = PopupManager.Instance.ActivePopup as GestureHintPopup;
        }

        if (MyMagicObjectCount > maxMagicObjectNumber)
        {
            if (gestureHintPopup != null) gestureHintPopup.ShowHint();
        }
        else
        {
            if (gestureHintPopup != null)
            {
                gestureHintPopup.ShowBar();
                gestureHintPopup.SetBarProgress(chargedTime / HandGestureTracker.instance.ChargingCheckTime);
            }
        }
    }

    private void OnLighthouseTracked(Lighthouse lighthouse, Pose pose, bool isCloseEnough)
    {
        scanSuccess = true;
        Invoke(nameof(ResetScanSuccess), 3);
    }

    private void OnHostingDomainSession(string domainId, string domainName)
    {
        ARAquariumUIController.Instance.HideScanner();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
        var popup = PopupManager.Instance.ActivePopup as HintPopup;
        popup.SetTextKeyTop("txt_hosting_domain_session");
        popup.SetDelayClose(2);
        SoundManager.Instance.Play("scan_success");
    }
    
    private void OnJoinedDomainSession(string domainId, string domainName)
    {
        ARAquariumUIController.Instance.HideScanner();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
        var popup = PopupManager.Instance.ActivePopup as HintPopup;
        popup.SetTextKeyTop("txt_joining_domain_session");
        popup.SetDelayClose(2);
        SoundManager.Instance.Play("scan_success");
    }
    
    private void OnDomainAssetsAvailable()
    {
        ARAquariumUIController.Instance.HideScanner();
        foreach (var fish in peerFishes)
        {
            Destroy(fish.CompWalker.Spline.gameObject);
            fish.CompWalker.Spline = fishTank.GetBezierPathForNavMesh(fish.transform.position, AukiSdkWrapper.Instance);
        }
    }

    private void OnJoined(Session session)
    {
        #if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent(DataCenter.Instance.JoinedVia == AnalyticsEnums.ValueQrCode
            ? AnalyticsEnums.EventJoinSpaceQrCode
            : AnalyticsEnums.EventJoinSpaceSpaceId);
        #endif
        if (scanSuccess)
        {
            ARAquariumUIController.Instance.FreezeScanner();
            PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
            var popup = PopupManager.Instance.ActivePopup as HintPopup;
            popup.SetTextKeyLower("txt_lighthouse_tracked");
            popup.SetDelayClose(2);
            SoundManager.Instance.Play("scan_success");
        }

        AddMyFishToServer();
        if (!AukiSdkWrapper.Instance.IsHost)
        {
            DeleteAllMagicObjects();
            ARAquariumUIController.Instance.ShowJoinSpaceFader();
            justJoined = true;
            Invoke(nameof(ResetJustJoined), 2);
        }
    }

    private void ResetJustJoined()
    {
        justJoined = false;
    }

    private void OnLeft()
    {
        DeleteMyFishFromServer();
        DeleteMyMagicObjectsFromServer();
        DestroyAllPeerFishes();
        DeleteAllMagicObjects();
        HandGestureTracker.Instance.ResetTime();
    }

    private void OnParticipantJoined(Participant participant)
    {
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventBeenJoined);
    }

    private void OnEntityDeleted(uint aukiEntityId)
    {
        // only delete peer's fish and magic objects
        if (entityObjectDic.TryGetValue(aukiEntityId, out GameObject go))
        {
            if (go == null) return; // in case of scene unloaded
            MagicObject magicObject = go.GetComponent<MagicObject>();
            if (magicObject != null && !magicObject.Owned)
            {
                Logger.Log($"[{GetType().Name}] OnEntityDeleted: start delete magic object {aukiEntityId}");
                StartCoroutine(magicObject.Disappear()); // will remove form list and dic in magic object's OnDestroy
            }
            else
            {
                FishEntity fishEntity = go.GetComponent<FishEntity>();
                if (fishEntity != null && !fishEntity.Owned)
                {
                    Logger.Log($"[{GetType().Name}] OnEntityDeleted: delete fish {aukiEntityId}");
                    entityObjectDic.Remove(aukiEntityId);
                    Destroy(fishEntity.gameObject);

                    // show message
                    PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
                    var hintText = Localization.Get("txt_message_friend_left_hint");
                    hintText = hintText.Replace("{0}", fishEntity.OwnerName);
                    var popup = PopupManager.Instance.ActivePopup as HintPopup;
                    popup.SetTextTop(hintText);
                    popup.SetDelayClose(2);
                }
            }
        }
    }

    private void OnEntityAction(EntityAction action)
    {
        switch (action.Name)
        {
            case EntityActionNameEnum.CreateHeart:
            case EntityActionNameEnum.CreateTorus:
            case EntityActionNameEnum.CreateBowl:
            {
                Logger.Log($"[{GetType().Name}] OnEntityAction: {action.Name} {action.EntityId}");
                Entity aukiEntity = AukiSdkWrapper.Instance.GetEntityById(action.EntityId);
                var (ownerName, position) = DataConverter.BytesToAddMagicEntity(action.Data);
                MagicObjectTypeEnum magicObjectType =
                    action.Name == EntityActionNameEnum.CreateHeart
                        ? MagicObjectTypeEnum.Heart
                        : action.Name == EntityActionNameEnum.CreateTorus
                            ? MagicObjectTypeEnum.Torus
                            : MagicObjectTypeEnum.Bowl;
                MagicObject magicObject = CreateMagicObject(position, magicObjectType, ownerName);
                magicObject.AukiEntity = aukiEntity;
                entityObjectDic.Add(aukiEntity.Id, magicObject.gameObject);
                return;
            }
            case EntityActionNameEnum.CreateFish:
            {
                Logger.Log($"[{GetType().Name}] OnEntityAction: create fish {action.EntityId}");
                Entity aukiEntity = AukiSdkWrapper.Instance.GetEntityById(action.EntityId);
                var (ownerName, imageUrl, position) = DataConverter.BytesToAddFishEntity(action.Data);
                FishEntity fishEntity = CreateFish(ownerName, imageUrl, position);
                fishEntity.AukiEntity = aukiEntity;
                entityObjectDic.Add(aukiEntity.Id, fishEntity.gameObject);
                OnFishAddedToServer?.Invoke(fishEntity);
                // show message
                if (!justJoined)
                {
                    PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
                    var hintText = Localization.Get("txt_message_friend_join_hint");
                    hintText = hintText.Replace("{0}", ownerName);
                    var popup = PopupManager.Instance.ActivePopup as HintPopup;
                    popup.SetTextTop(hintText);
                    popup.SetDelayClose(2);
                }

                return;
            }
            // delete my magic object
            case EntityActionNameEnum.DeleteEntity
                when entityObjectDic.ContainsKey(action.EntityId)
                     && AukiSdkWrapper.Instance.GetEntityById(action.EntityId).ParticipantId == AukiSdkWrapper.Instance.MyPeerId:
            {
                MagicObject magicObject = entityObjectDic[action.EntityId].GetComponent<MagicObject>();
                if (entityObjectDic[action.EntityId].GetComponent<MagicObject>() != null)
                {
                    Logger.Log($"[{GetType().Name}] OnEntityAction: start delete magic object {action.EntityId}");
                    AukiSdkWrapper.Instance.DeleteEntity(action.EntityId);
                    StartCoroutine(magicObject.Disappear());
                }

                break;
            }
        }
    }

    private void OnEntityUpdatePose(Entity aukiEntity) { }

    #endregion

    #region Private Methods

    private void InitEnvironment()
    {
        if (seaGO != null) seaGO.SetActive(SystemExtension.isFastCPU());
        if (waterLightGO != null) waterLightGO.SetActive(SystemExtension.isFastCPU());
        if (bubblesGO != null) bubblesGO.SetActive(SystemExtension.isFastCPU());
        if (fadeInGO != null) fadeInGO.SetActive(true);

        underWaterSoundThread = PlayUnderWaterSound();
        StartCoroutine(underWaterSoundThread);
        StartCoroutine(DelayInitEnvironment(0.1f));
    }

    private IEnumerator DelayInitEnvironment(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance.SetBGBListAndPlay(new[] { "dreamweaver" });
        if (fadeInGO != null)
        {
            fadeInGO.SetActive(false);
        }

        if (fadeOutGO != null)
        {
            fadeOutGO.SetActive(true);
        }

        GameSave.NumEnterARMode++;

        //enable blue layout and water filter
        EnableOceanEffect(true);
        Vector3 dir = Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up);
        dir.x = 0;
        GenerateFishTank(arCamera.transform.position + new Vector3(0, -1.5f, 0), Quaternion.Euler(dir));
    }

    private void GenerateFishTank(Vector3 position, Quaternion rotation)
    {
        AquariumData aquariumData = DataCenter.Instance.GetAquarium(GameSave.SelectedAquarium);
        GameObject tankModel = Instantiate(Resources.Load(GameSetting.ModelPrefabPath + aquariumData.Data.ARGround) as GameObject, position, rotation);

        //manually init
        fishTank = tankModel.GetComponentInChildren<FishTank>();
        fishTank.InitFishTank(true);

        Transform cameraTransform = arCamera.transform;
        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * 1.5f;
        spawnPosition.y = cameraTransform.position.y;
        CreateMyFish(spawnPosition);
        CreateBackgroundFishes();

        //set default size
        tankModel.transform.localScale = Vector3.one * aquariumData.Data.InitScale;
        fishTank.SetTankSize(tankModel.transform.localScale.x);
    }

    private void CreateMyFish(Vector3 position)
    {
        CreateFish(UserName, DataCenter.Instance.CurrentArtwork.Url, position, true);
    }

    private void CreateBackgroundFishes()
    {
        FishEntity fishEntity;

        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 3f, new Vector3(0, 1.5f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 4f, new Vector3(0, 2f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 5f, new Vector3(0, 2.5f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 6f, new Vector3(0, 3f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;

        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 3.5f, new Vector3(0, 1.7f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 4.5f, new Vector3(0, 2.2f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 5.5f, new Vector3(0, 2.7f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
        fishEntity = fishTank.GenerateCircularFish("Nemo", "Nemo-Mandala", 6.5f, new Vector3(0, 3.2f, 0));
        fishEntity.CompMagicObjectChecker.enabled = false;
    }

    private IEnumerator PlayUnderWaterSound()
    {
        while (true)
        {
            SoundManager.Instance.Play("underwater");
            yield return new WaitForSeconds(25);
        }
    }

    private void ResetScanSuccess() => scanSuccess = false;

    private string CutString(string text, int len, string ellipticalString = "")
    {
        var ascii = new System.Text.ASCIIEncoding();
        byte[] s = ascii.GetBytes(text);

        int byteLen = 0;

        for (int i = 0; i < s.Length; i++)
        {
            if ((int)s[i] == 63)
            {
                byteLen += 1;
            }

            byteLen += 1;
        }

        if (byteLen <= len)
        {
            return text;
        }

        int resultLen = 0;
        string resultString = "";

        for (int i = 0; i < s.Length; i++)
        {
            if ((int)s[i] == 63)
            {
                resultLen += 1;
            }

            resultLen += 1;

            if (resultLen > len)
            {
                break;
            }

            resultString += text.Substring(i, 1);
        }

        resultString += ellipticalString;
        return resultString;
    }

    #endregion
}