using System;
using Firebase.Analytics;
using PaintCraft.Controllers;
using ProGamer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModelPreviewPopup : SimplePopup
{
    [SerializeField] private UIEventButton uiEventButton;
    [SerializeField] private GameObject hatcheryIcon;
    [SerializeField] private GameObject backIcon;
    [SerializeField] private Text backToAquariumText;
    [SerializeField] public Button eventButton;
    [SerializeField] public Button editButton;
    [SerializeField] public Button removeButton;
    [SerializeField] public GameObject hatcheryNode;
    [SerializeField] public GameObject metaverseNode;
    [SerializeField] public GameObject replaceButton;
    [SerializeField] public GameObject lockedReplaceButton;

    [Header("Preview Model")]
    [SerializeField] private Transform container;

    [SerializeField] protected float rotateSpeed = 0.2f;

    private GameObject model;
    private ImageData currentImageData;

    protected override void InitPopup()
    {
        hatcheryNode.SetActive(GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId);
        metaverseNode.SetActive(GameSave.SelectedAquarium != GameSetting.HatcheryAquariumId);
        EventStatus status = DataCenter.Instance.CurrentArtwork.GetStatus();
        editButton.gameObject.SetActive(status == EventStatus.Active || status == EventStatus.Coming);
        SetBackButton();
        SetupEventButton();
        UpdateReplaceButton();
        base.InitPopup();
        LoadModel();
        currentImageData = DataCenter.Instance.CurrentArtwork;
    }

    private void OnEnable()
    {
        Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenPreview);

        PopupDidShowEvent += ShowTutorial;
        ScratchInput.OnUserScratching += OnUserScratching;
        ServerCenter.Instance.EventDataUpdatedEvent += OnEventDataUpdated;
    }

    protected override void OnDisable()
    {
        Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenPreview);
        ScratchInput.OnUserScratching -= OnUserScratching;
        ServerCenter.Instance.EventDataUpdatedEvent -= OnEventDataUpdated;
        base.OnDisable();
        PopupDidShowEvent -= ShowTutorial;
    }

    public override void Close()
    {
        NativeHelper.GenerateImpactFeedback();
        // close ReplaceFishListPopup
        if (PopupManager.Instance.ActivePopup != this)
        {
            PopupManager.Instance.ActivePopup.Close();
        }

        base.Close();
        DestroyModel();
        // may close with replace fish list panel opened
        if (currentImageData != DataCenter.Instance.CurrentArtwork)
        {
            ImageDataManager.Instance.SaveDataToDisk();
            ImageDataManager.Instance.FillImageDataLists(true);
        }
    }

    public void EditImage()
    {
        Close();
        if (SceneManager.GetActiveScene().name == "Aquarium")
        {
            AquariumUIController.Instance.DrawFromArtwork();
        }
    }

    public void RemoveArtwork()
    {
        PopupManager.Instance.ShowConfirmPopup("txt_confirm_remove_title", null, OnConfirmRemoveArtwork, null, true);
    }

    public void ShowEventInfo()
    {
        NativeHelper.GenerateImpactFeedback();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.EventInfoPopup, true);
    }

    public void LaunchMetaverse()
    {
        AquariumUIController.Instance.EnterARAquarium();
    }

    public void ShowReplaceFish()
    {
        NativeHelper.GenerateImpactFeedback();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ReplaceFishListPopup, true);
        var replaceFishListPopup = (UIReplaceFishListPopup)PopupManager.Instance.ActivePopup;
        replaceFishListPopup.ModelPreviewPopup = this;
    }

    public void ShowPurchaseAccessory()
    {
        PopupManager.Instance.QuickShowPopup(PopupPrefab.PurchasePopup, true);
        var popup = PopupManager.Instance.ActivePopup as PurchasePopup;
        popup.ShowAccessoryItem();
        popup.PurchaseSuccessCallback = () =>
        {
            UpdateReplaceButton();
        };
    }

    public void SubmitArtwork()
    {
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventClickSubmit);
        if (ServerCenter.Instance.VersionStats == VersionStats.Latest)
        {
            DateTime lastTime = DateTime.Parse(GameSave.SubmitArtworkTime);
            // if ((DateTime.UtcNow - lastTime).TotalHours < 4)
            if ((DateTime.UtcNow - lastTime).TotalSeconds < 60)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.SubmitCooldownPopup, true);
            }
            else
            {
                if (!ServerCenter.Instance.IsLoggedIn)
                {
                    PopupManager.Instance.QuickShowPopup(PopupPrefab.SignInWithApplePopup, true);
                    var popup = PopupManager.Instance.ActivePopup as SignInWithApplePopup;
                    popup.OnSuccess = () =>
                    {
                        PopupManager.Instance.QuickShowPopup(PopupPrefab.SubmitPopup, true);
                        ((SubmitPopup)PopupManager.Instance.ActivePopup).SetAppleId(ServerCenter.Instance.AppleName);
                    };
                }
                else
                {
                    PopupManager.Instance.QuickShowPopup(PopupPrefab.SubmitPopup, true);
                    ((SubmitPopup)PopupManager.Instance.ActivePopup).SetAppleId(ServerCenter.Instance.AppleName);
                }
            }
        }
        else if (ServerCenter.Instance.VersionStats == VersionStats.NeedUpdate)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.NewVersionPopup, true);
            var popup = PopupManager.Instance.ActivePopup as NewVersionPopup;
            popup.SetUpdateInfo(true);
        }
    }

    public void Back()
    {
        if (SceneManager.GetActiveScene().name == "ColoringBook")
        {
            bool isNewImage = false;
            if (ImageDataManager.Instance.NewImageData != null && !ImageDataManager.Instance.NewImageData.Submitted)
            {
                ImageDataManager.Instance.AddNewImageDataIntoAquarium(true, false); // confirm to add the new fish into the aquarium
                ImageDataManager.Instance.NewImageData = null;
                GameSave.NewFishAddedFlag = true;
                isNewImage = true;
            }

            CanvasController canvasController = FindObjectOfType<CanvasController>();
            if (isNewImage || canvasController.UndoManager.HasUndo())
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
                var popup = PopupManager.Instance.ActivePopup as HintPopup;
                popup.SetTextKeyLower("txt_artwork_saved");
                popup.SetDelayClose(1, () =>
                {
                    SceneTransitHandler.LoadScene("Aquarium");
                });
            }
            else
            {
                SceneTransitHandler.LoadScene("Aquarium");
            }
        }
        else
        {
            Close();
        }
    }

    public void UpdateReplaceButton()
    {
        bool isDefaultFish = DataCenter.Instance.CurrentArtwork.ImageFile == GameSetting.DefaultFish;
        replaceButton.SetActive(DataCenter.Instance.IsAccessoryPurchased() && !isDefaultFish);
        lockedReplaceButton.SetActive(!DataCenter.Instance.IsAccessoryPurchased() && !isDefaultFish);
    }

    public void UpdateModel()
    {
        DestroyModel();
        LoadModel();
    }

    private static void ShowTutorial()
    {
        if (GameSave.ShowTutorialPreview)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMessagePopup, true);
            var popup = PopupManager.Instance.ActivePopup as TutorialMessagePopup;
            popup.SetTutorial(TutorialType.Preview);
        }
    }

    private void OnConfirmRemoveArtwork()
    {
        ImageDataManager.Instance.RemoveImageData(DataCenter.Instance.CurrentArtwork, true, true, true);
        Close();
        if (SceneManager.GetActiveScene().name == "Aquarium")
        {
            var fishTank = FindObjectOfType<FishTank>();
            if (fishTank == null) return;
            FishEntity fishEntity = fishTank.FindFish(DataCenter.Instance.CurrentArtwork);
            if (fishEntity != null)
            {
                fishTank.RemoveFish(fishEntity);
            }
        }
        else
        {
            SceneTransitHandler.LoadScene("Aquarium");
        }
    }

    private void LoadModel()
    {
        CharacterData characterData;
        ImageData imageData = DataCenter.Instance.CurrentArtwork;

        if (DataCenter.Instance.CharacterDataDic.TryGetValue(imageData.GetModelName(), out characterData))
        {
            Debug.Log("Load: " + FishTank.ModelPath + characterData.PrefabPath);

            model = ReuseableObjectManager.Instantiate(FishTank.ModelPath + characterData.FishTankModelName, Vector3.zero, Quaternion.identity);
            model.AddComponent<RectTransform>();
            model.transform.SetParent(container);
            model.transform.localPosition = new Vector3(0, characterData.PreviewYOffset, 0);
            model.transform.localEulerAngles = new Vector3(0, characterData.PreviewYAngle, 0);
            model.transform.localScale = Vector3.one * characterData.PreviewSize;
            ChangeLayersRecursively(model.transform, 24); // UIModel layer

            Texture2D skin = TextureCache.Instance.Load(imageData.ImageFile, true);
            model.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
        }
    }

    private void ChangeLayersRecursively(Transform xform, int layer)
    {
        for (int i = 0; i < xform.childCount; i++)
        {
            xform.GetChild(i).gameObject.layer = layer;
            ChangeLayersRecursively(xform.GetChild(i), layer);
        }
    }

    private void DestroyModel()
    {
        Destroy(model);
    }

    private void SetBackButton()
    {
        if (SceneManager.GetActiveScene().name == "ColoringBook")
        {
            hatcheryIcon.SetActive(true);
            backIcon.SetActive(false);
            backToAquariumText.text = Localization.Get("txt_hatchery");
        }
        else
        {
            hatcheryIcon.SetActive(false);
            backIcon.SetActive(true);
            backToAquariumText.text = Localization.Get("txt_back");
        }
    }

    private void SetupEventButton()
    {
        uiEventButton.SetCanSubmit(true);

        var data = DataCenter.Instance.CurrentArtwork;
        if (data.Submitted)
        {
            uiEventButton.SetEvent(EventStatus.Inactive, 0);
        }
        else
        {
            uiEventButton.SetEvent(data.GetStatus(), data.GetTimeSpan().Days);
        }
    }

    private void OnUserScratching(Vector2 force)
    {
        model.transform.eulerAngles += new Vector3(0, -force.x * rotateSpeed, 0);
    }

    private void OnEventDataUpdated()
    {
        SetupEventButton();
        EventStatus status = DataCenter.Instance.CurrentArtwork.GetStatus();
        editButton.gameObject.SetActive(status == EventStatus.Active || status == EventStatus.Coming);
    }
}