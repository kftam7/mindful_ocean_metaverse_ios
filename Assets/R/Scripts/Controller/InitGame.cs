//#define TUTORIAL

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Firebase;
using Firebase.Analytics;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProGamer;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class InitGame : Network.Singleton<InitGame>
{
    public CanvasGroup UIGroup;
    public Animator CameraAnimator;
    public Animator DeskAnimator;
    public GameObject BookPreview;

    string HomeScene = "Aquarium";

    void Awake()
    {
        Debug.Log($"[{GetType().Name}] init game...");
        DontDestroyOnLoad(gameObject);
        // todo: delete the saved data of the old version to avoid loading
        // the old aquarium after installing the new version. this is a
        // temporary measure because compatibility with old versions may be
        // considered.
        if (!GameSave.NewVersion)
        {
            PlayerPrefs.DeleteAll();
            GameSave.NewVersion = true;
        }

        InitUnityServices();
        CheckToCreateAquariums();
        if (NetworkSettings.Instance.IsMock || Application.isEditor)
        {
            MockServer.Instance.Init();
        }

        WebConnectionChecker.Instance.Init(NetworkSettings.Instance.GetCheckingConnectionUrl());
        TextureCache.Instance.Init();
        ServerCenter.Instance.Init();
        ServerResponseRecorder.Instance.Init();
        ImageDataManager.Instance.Init();
        CheckToCreateDefaultFish();

        // SetPreview();
    }

    private void Start()
    {
        #if !UNITY_EDITOR
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        });
        #endif
    }
    /* by nxa
    public void Play()
    {

        ProGamer.SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        StartCoroutine(AlphaFadeOut(10));
        CameraAnimator.CrossFade("DeskCamera", 0);
        DeskAnimator.CrossFade("cleardesk", 0);
    }
    */

    IEnumerator AlphaFadeOut(int step)
    {
        for (int i = 1; i <= step; i++)
        {
            yield return new WaitForSeconds(0.02f);
            UIGroup.alpha = 1 - (float)i / step;
        }
    }

    public void EnterGame()
    {
        Debug.Log($"[{GetType().Name}] EnterGame()");
#if UNITY_IOS && !UNITY_EDITOR
        NativeHelper.PrepareImpactFeedbackGenerator();
        //PlayerSettings.iOS.hideHomeButton = false;
#endif
#if TUTORIAL
        if (!GameSave.IsTutorialFinished)
        {
            TutorialController.Instance.StartTutorial("BeginnerTutorial", GameSetting.BeginnerTutorial);
        }
        else
        {
            HomeScene = "Aquarium";
            if (!GameSave.IsPrinterTutorialFinished)
            {
                if (!string.IsNullOrEmpty(GameSave.LastTutorialFinishTime))
                {
                    System.DateTime lastTime;

                    if (System.DateTime.TryParse(GameSave.LastTutorialFinishTime, out lastTime))
                    {
#if FORCE_PRINTING_TUTORIAL
                        if ((System.DateTime.UtcNow - lastTime).Days >= 0)
#else
                        if ((System.DateTime.UtcNow - lastTime).Days >= 1)
#endif

                        {
                            Debug.Log("Show printer tutorial");

                            //GameSave.IsPrinterTutorialFinished = true;
                            //TutorialController.Instance.StartTutorial("PrinterTutorial", GameSetting.PrinterTutorial);
                        }
                    }
                }
            }
        }
#endif
    }

    IEnumerator LoadHomeScene()
    {
        SceneTransitHandler.SceneTransitManually(HomeScene);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(HomeScene);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void SetPreview()
    {
        List<AquariumData> dataList = new List<AquariumData>(DataCenter.Instance.AquariumsData.Values);

        if (dataList.Count > 0 && !string.IsNullOrEmpty(dataList[0].SnapShotPath))
        {
            Texture t = ImageDataWriter.LoadImage(dataList[0].SnapShotPath, false);

            // PlacementManager.ColoringGameObject(BookPreview, "Unlit/Alpha Map", t);
        }
    }

    private async void InitUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e) { }
    }

    private void CheckToCreateAquariums()
    {
        bool hasHatchery = false;
        bool hasMetaverse = false;

        foreach (var data in DataCenter.Instance.AquariumsData.Values)
        {
            if (data.AquariumName == GameSetting.HatcheryAquariumName) hasHatchery = true;
            if (data.AquariumName == GameSetting.MetaverseAquariumName) hasMetaverse = true;
        }

        if (!hasHatchery)
        {
            DataCenter.Instance.AddAquarium(new AquariumData(GameSetting.HatcheryAquariumId,
                GameSetting.HatcheryAquariumName,
                DataCenter.Instance.CollectionDataDic[GameSetting.HatcheryAquariumTheme], 0));
        }

        if (!hasMetaverse)
        {
            DataCenter.Instance.AddAquarium(new AquariumData(GameSetting.MetaverseAquariumId,
                GameSetting.MetaverseAquariumName,
                DataCenter.Instance.CollectionDataDic[GameSetting.MetaverseAquariumTheme], 1));
        }
    }

    private void CheckToCreateDefaultFish()
    {
        if (GameSave.DefaultFishAdded) return;
        Debug.Log($"[{GetType().Name}] Create default fish.");
        Texture2D texture = ImageDataWriter.LoadImage(Path.Combine(Application.streamingAssetsPath, GameSetting.DefaultFish), false);
        ImageDataWriter.SaveImage(texture, GameSetting.DefaultFish);

        var data = new ImageData
        {
            CaptureTime = DateTime.UtcNow,
            Submitted = true,
            IsNft = true,
            Unlocked = true,
            IsDefault = true,
            ImageFile = GameSetting.DefaultFish,
            InAquarium = true
        };
        ImageDataManager.Instance.AddImageData(data, true, false); // not in aquarium for now
        GameSave.DefaultFishAdded = true;
    }
}