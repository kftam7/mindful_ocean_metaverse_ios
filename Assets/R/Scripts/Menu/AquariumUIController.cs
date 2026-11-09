using System;
using System.Collections;
using System.Collections.Generic;
using PaintCraft.Canvas;
using PaintCraft.Canvas.Configs;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;
using UnityEngine.Assertions;

public class AquariumUIController : SceneInstance<AquariumUIController>
{
    [SerializeField] protected float rotateSpeed = 0.05f;
    [SerializeField] protected GameObject hiddenGroup;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected CanvasGroup listCanvasGroup;
    [SerializeField] protected GameObject newMemberMessage;
    [SerializeField] protected Animator scanTransit;
    [SerializeField] protected GameObject fadeOut;
    [SerializeField] protected GameObject fishListPanel;
    [SerializeField] protected Button arBtn;
    [SerializeField] protected Button homeBtn;
    [SerializeField] protected UIAddFishButton addFishButton;
    private Transform tankModel;
    private AquariumData currentAquarium;
    private bool IsHatchery => GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId;

    public delegate void LeaveSceneCallback();

    public DateTime startTime;

    void Start()
    {
        startTime = DateTime.UtcNow;

        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        arBtn.onClick.AddListener(EnterARAquarium);
        homeBtn.onClick.AddListener(GoHome);
        addFishButton.Init();
        if (IsHatchery)
        {
            Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenHatchery);
            addFishButton.Button.onClick.AddListener(AddFish);
            arBtn.gameObject.SetActive(true);
        }
        else
        {
            Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenMetaverse);
            addFishButton.Button.onClick.AddListener(SwitchFishList);
            arBtn.gameObject.SetActive(false);
        }

        currentAquarium = DataCenter.Instance.GetSelectedAquarium();

        if (currentAquarium != null)
        {
            GameObject go = Instantiate(Resources.Load("Models/Tanks/" + currentAquarium.Data.Ground) as GameObject, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(null);
            go.transform.localScale = Vector3.one;
            FishTank fishTank = go.GetComponentInChildren<FishTank>();
            fishTank.InitFishTank();

            fishTank.CreateFishes(currentAquarium.DefaultCreature, currentAquarium.DefaultSkin);
            fishTank.CreateFishes(ImageDataManager.Instance.GetInAquariumImageDataList(currentAquarium.AquariumId), 0, Vector3.zero);

            tankModel = go.transform;

            //coloring
            string groundSkin = currentAquarium.GroundSkin;
            if (!string.IsNullOrEmpty(groundSkin))
            {
                Texture2D t = ImageDataWriter.LoadImage(groundSkin, false);
                if (t != null)
                {
                    // PlacementManager.ColoringGameObject(go, "Unlit/Alpha Map", t);
                }
            }

            if (SceneTransitHandler.PreviousScene == "AquariumManageScene" || SceneTransitHandler.PreviousScene == "InitScene")
            {
                fadeOut.SetActive(true);
            }
        }

        if (!string.IsNullOrEmpty(currentAquarium.NewMember))
        {
            scanTransit.CrossFade("ScanTransition_FadeIn", 0);

            ShowObject(6f, newMemberMessage);
            currentAquarium.NewMember = "";
            DataCenter.Instance.SaveAquarium();

            AnalyticsLogCenter.LogEvent("created_creature_amount", new Dictionary<string, object>()
            {
                { "amount", ImageDataManager.Instance.ImageDataList.Count }
            });
        }
        else
        {
            TakePreview();
        }

        // avoid tutorial bug
        ButtonTool.PausingInteractable(arBtn, 1f);
        ButtonTool.PausingInteractable(homeBtn, 1f);
        ButtonTool.PausingInteractable(addFishButton.Button, 1f);

        //ShowHiddenButton();
        if (!GameSave.DidShowColorPageReview && GameSave.NumDrawColorPage >= 15)
        {
            NativeHelper.OpenInAppReview();
            GameSave.DidShowColorPageReview = true;
        }

        if (!GameSave.DidShowARModeReview && GameSave.NumEnterARMode >= 10)
        {
            NativeHelper.OpenInAppReview();
            GameSave.DidShowARModeReview = true;
        }

        // Show new fish added message
        if (GameSave.NewFishAddedFlag)
        {
            GameSave.NewFishAddedFlag = false;
            PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
            var popup = PopupManager.Instance.ActivePopup as HintPopup;
            popup.SetTextKeyLower("txt_fish_added");
            popup.SetDelayClose(2);
        }

        if (IsHatchery && GameSave.ShowTutorialHatchery)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMessagePopup, true);
            var popup = PopupManager.Instance.ActivePopup as TutorialMessagePopup;
            popup.SetTutorial(TutorialType.Hatchery);
        }
        else if (!IsHatchery && GameSave.ShowTutorialMetaverse)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMessagePopup, true);
            var popup = PopupManager.Instance.ActivePopup as TutorialMessagePopup;
            popup.SetTutorial(TutorialType.Metaverse);
        }

        addFishButton.gameObject.SetActive(!IsHatchery);
        InvokeRepeating(nameof(UpdateVersion), 0.1f, 5f);
        InvokeRepeating(nameof(UpdateEvents), 0.1f, 5f);
    }

    public void TakePreview()
    {
        StartCoroutine(AquariumSnapShot(null));
    }

    void OnEnable()
    {
        ServerCenter.Instance.EventDataUpdatedEvent += OnEventDataUpdated;
        ScratchInput.OnUserScratching += OnUserScratching;
    }

    void OnDisable()
    {
        ServerCenter.Instance.EventDataUpdatedEvent -= OnEventDataUpdated;
        ScratchInput.OnUserScratching -= OnUserScratching;
    }

    private void OnDestroy()
    {
        if (IsHatchery)
        {
            Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenHatchery);
        }
        else
        {
            Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenMetaverse);
        }

        Resources.UnloadUnusedAssets();
    }

    protected float rotateForceInertia = 0f;

    public void OnUserScratching(Vector2 force)
    {
        tankModel.eulerAngles += new Vector3(0, -force.x * rotateSpeed, 0);
        rotateForceInertia = -force.x;
    }

    void Update()
    {
        if (tankModel != null && Math.Abs(rotateForceInertia) > 0)
        {
            tankModel.eulerAngles += new Vector3(0, rotateForceInertia * rotateSpeed, 0);
            rotateForceInertia /= 1.2f;
            if (Math.Abs(rotateForceInertia) < 0.0001f)
            {
                rotateForceInertia = 0;
            }
        }
    }

    public void AddFish()
    {
        if (ServerCenter.Instance.VersionStats == VersionStats.Latest)
        {
            DrawFromEvent();
        }
        else if (ServerCenter.Instance.VersionStats == VersionStats.NeedUpdate)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.NewVersionPopup, true);
            var popup = PopupManager.Instance.ActivePopup as NewVersionPopup;
            popup.SetUpdateInfo(true);
        }
    }

    public void DrawFromEvent()
    {
        EventItemResultData eventData = ServerCenter.Instance.GetCurrentEventData();
        string templateFileName = ImageFileNameUtils.GetFullFileNameFromUrl(eventData.image);
        ServerCenter.Instance.GetImageFromCacheOrUrl(eventData.image, false, texture =>
        {
            if (texture != null)
            {
                var data = new ImageData(templateFileName)
                {
                    EventId = eventData.eventId,
                    Type = eventData.type,
                    CaptureTime = DateTime.UtcNow,
                    Submitted = false,
                    InAquarium = false
                };
                DataCenter.Instance.CurrentArtwork = data;
                SetPageConfigForDraw(texture, data.GetModelName());
                fishListPanel.SetActive(false); // don't update fish models.
                ImageDataManager.Instance.AddImageData(data, true, false); // not in aquarium for now
                ImageDataManager.Instance.NewImageData = data;
                // don't CheckFishNumberInAquariumsToLimit() here to remove older fish right now,
                // in case of that the new artwork is removed in preview panel in coloring scene.
            }
        });
    }

    public void DrawFromArtwork()
    {
        ImageData imageData = DataCenter.Instance.CurrentArtwork;
        Assert.IsNotNull(imageData.GetTemplateFile());
        string templateUrl = ServerCenter.Instance.GetTemplateUrlByFileName(imageData.GetTemplateFile());
        if (string.IsNullOrEmpty(templateUrl)) return;
        ServerCenter.Instance.GetImageFromCacheOrUrl(templateUrl, false, texture =>
        {
            if (texture != null)
            {
                SetPageConfigForDraw(texture, imageData.GetModelName());
            }
        });
    }

    public void SetPageConfigForDraw(Texture2D texture, string modelName)
    {
        AdvanceColoringPageConfig pageConfig = ScriptableObject.CreateInstance<AdvanceColoringPageConfig>();
        pageConfig.UniqueId = "1"; // todo: no use anymore

        //Check device ram

        int memorySize = SystemInfo.systemMemorySize;

        if (memorySize <= 1024)
        {
            Debug.Log("Reduce texture dimension due to the memory size - " + memorySize);
            ImageDataWriter.Scale(texture, 1024, 1024);
        }

        pageConfig.SetColoringTexture(texture, null);

        //Will not save progress if without name
        pageConfig.name = "ColoringPaper";

        AppData.SelectedPageConfig = pageConfig;
        AppData.SelectedPageModel = modelName;

        //Clear memory
        TextureCache.Instance.ClearCache();
        SpriteDicCenter.Instance.ReleaseMemory();
        GL.Clear(true, true, Color.black);

        SceneTransitHandler.LoadScene("ColoringBook", true);
    }

    public void EnterARAquarium()
    {
        NativeHelper.GenerateImpactFeedback();
        Debug.Log("EnterARAquarium");
        ButtonTool.PausingInteractable(arBtn, 0.3f);
        AnalyticsLogCenter.LogEvent("enter_ar_mode");

        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        if (!GameSave.HasShownCameraPermission)
        {
            GameSave.HasShownCameraPermission = true;
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_camera", EnterARAquarium, true);
        }
        else
        {
#if UNITY_IOS && !UNITY_EDITOR
            if(NativeHelper.GetCameraPermission() == PermissionStatus.Undetermined)
            {
                NativeHelper.onCameraPermissionUpdated += (permission) =>
                {
                    NativeHelper.onCameraPermissionUpdated = null;
                    EnterARAquarium();
                };
                NativeHelper.RequestCameraPermission();
                return;
            }
            else if(NativeHelper.GetCameraPermission() == PermissionStatus.Denied)
            {
                PopupManager.Instance.ShowPermissionPopup("txt_permissions_title", "txt_permission_camera", ()=>
                {
                    NativeHelper.OpenSetting();
                }, ()=>
                {

                }, true);
                return;
            }
#endif
            LoadScene(IsHatchery ? "ARHatchery" : "ARMetaverse");
        }
    }

    public void EnterARCapture()
    {
        AnalyticsLogCenter.LogEvent("enter_ar_capture");

        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        if (!GameSave.HasShownCameraPermission)
        {
            GameSave.HasShownCameraPermission = true;
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_camera", EnterARCapture, true);
        }
    }

    public void GoHome()
    {
        NativeHelper.GenerateImpactFeedback();
        ButtonTool.PausingInteractable(homeBtn, 1f);
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        LoadScene("AquariumManageScene");
    }

    public void SwitchFishList()
    {
        fishListPanel.GetComponent<UIFishListPanel>().SwitchPanel();
    }

    IEnumerator showingThread;

    public void ShowHiddenButton()
    {
        if (hiddenGroup.activeSelf)
        {
            hiddenGroup.SetActive(true);
            if (showingThread != null)
            {
                StopCoroutine(showingThread);
            }

            showingThread = ShowingShownButton();
            StartCoroutine(showingThread);
        }
        else
        {
            // avoid tutorial bug
            ButtonTool.PausingInteractable(arBtn, 0.5f);
            ButtonTool.PausingInteractable(homeBtn, 0.5f);
            ButtonTool.PausingInteractable(addFishButton.Button, 0.5f);

            if (showingThread != null)
            {
                StopCoroutine(showingThread);
            }

            showingThread = ShowingHiddenButton();
            StartCoroutine(showingThread);
        }
    }

    public void HideHiddenButton()
    {
        if (showingThread != null)
        {
            StopCoroutine(showingThread);
        }

        if (hiddenGroup.activeSelf)
        {
            hiddenGroup.SetActive(false);
        }
    }

    IEnumerator ShowingHiddenButton()
    {
        hiddenGroup.SetActive(true);
        yield return new WaitForSeconds(5f);
        hiddenGroup.SetActive(false);
    }

    IEnumerator ShowingShownButton()
    {
        hiddenGroup.SetActive(true);
        hiddenGroup.GetComponent<Animator>().Play("ShowHidenButton", 0, 0.2f);
        yield return new WaitForSeconds(4f);
        hiddenGroup.SetActive(false);
    }

    public void LoadScene(string target)
    {
        LeaveSceneCallback callback = () =>
        {
            SceneTransitHandler.LoadScene(target);
        };

        StartCoroutine(AquariumSnapShot(callback));
    }

    IEnumerator AquariumSnapShot(LeaveSceneCallback callback)
    {
        SetUIActive(false);
        fadeOut.SetActive(false);

        yield return new WaitForEndOfFrame();
        if (PopupManager.Instance.ActivePopup == null)
        {
            //take screen shot when leave scene

            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
            texture.Apply();

            if (!string.IsNullOrEmpty(currentAquarium.SnapShotPath))
            {
                Debug.Log("Remove SnapShot");
                ImageDataWriter.RemoveImage(currentAquarium.SnapShotPath);
            }

            currentAquarium.SnapShotPath = ImageDataManager.Instance.GetAquariumSnapshotFilePath(currentAquarium.AquariumId);
            var data = new ImageData
            {
                ImageFile = currentAquarium.SnapShotPath,
                CaptureTime = DateTime.UtcNow
            };

            ImageDataWriter.SaveImage(texture, data.ImageFile, false);
            DestroyImmediate(texture);
            ImageDataManager.Instance.AddImageData(data, true, false);

            DataCenter.Instance.SaveAquarium();
        }

        SetUIActive(true);

        if (callback != null)
        {
            callback();
        }
    }

    public void SetUIActive(bool isActive)
    {
        canvasGroup.alpha = isActive ? 1 : 0;
        listCanvasGroup.alpha = isActive ? 1 : 0;
    }

    public void ShowObject(float time, GameObject target)
    {
        if (target)
        {
            StartCoroutine(ShowObjectForWhile(time, target));
        }
    }

    IEnumerator ShowObjectForWhile(float time, GameObject target)
    {
        target.SetActive(true);

        yield return new WaitForSeconds(time);

        target.SetActive(false);
    }

    private void UpdateVersion()
    {
        if (DataCenter.Instance.UpdateVersionPanelShowStats is PanelShowStats.IsShowing or PanelShowStats.Shown) return;
        ServerCenter.Instance.UpdateLatestVersion(
            _ =>
            {
                if (ServerCenter.Instance.VersionStats == VersionStats.NeedUpdate)
                {
                    PopupManager.Instance.QuickShowPopup(PopupPrefab.NewVersionPopup, true);
                    var popup = PopupManager.Instance.ActivePopup as NewVersionPopup;
                    popup.SetUpdateInfo(false);
                }
            },
            () => { });
    }

    private void UpdateEvents()
    {
        ServerCenter.Instance.UpdateEvents(null, () =>
        {
            if (GameSave.ShowTutorialMainFlow && !DataCenter.Instance.TutorialMainFlowShown)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMainFlowPopup, true);
            }
        });
    }

    private void OnEventDataUpdated()
    {
        EventItemResultData data = ServerCenter.Instance.GetCurrentEventData();
        int eventId = data?.eventId ?? -1;
        if (eventId != DataCenter.Instance.CurrentEventId)
        {
            ImageDataManager.Instance.FillImageDataLists(true);
            DataCenter.Instance.CurrentEventId = eventId;
        }

        if (IsHatchery)
        {
            if (data == null)
            {
                addFishButton.gameObject.SetActive(false);
                return;
            }

            if (data.GetStatus() is EventStatus.Coming or EventStatus.Active)
            {
                addFishButton.gameObject.SetActive(true);
            }
            else
            {
                addFishButton.gameObject.SetActive(false);
            }
        }
    }
}