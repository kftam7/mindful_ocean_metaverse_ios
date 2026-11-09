using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class ARRegionCaptureUI : SceneInstance<ARRegionCaptureUI>
{
    [SerializeField] protected GameObject uiContainer;
    [SerializeField] protected Sprite[] arModelShapes;
    [SerializeField] protected GameObject messageObject;
    [SerializeField] protected Text messageBox;
    [SerializeField] protected Button scanTutorialBtn;

    public GameObject SaveAndShareButton;
    protected bool[] cornerIndicator;
    public GameObject[] cornerIndicatorObject;

    public GameObject modelShowupEffect;
    public Animator hintsAnimator;

    public Animator sceneTansitAnimator;

    private const string ScanSuccessTxt = "Skin has scanned. You can scan a new one or move to your AR aquarium.";
    private const string OutOfBoundTxt = "Please make sure capture the whole skin.";

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnTrackingFound, OnTrackingFound);
        EventManager.StartListening(EventList.OnTrackingLost, OnTrackingLost);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnTrackingFound, OnTrackingFound);
        EventManager.StopListening(EventList.OnTrackingLost, OnTrackingLost);
    }

    //Status
    protected bool isObjectRecognized = false;
    protected bool isOutOfBound = true;
    protected bool changedTarget = false;


    protected string currentTargetName = "";

    void Start()
    {
        cornerIndicator = new bool[4];

        for (int i = 0; i < cornerIndicator.Length; i++)
        {
            cornerIndicator[i] = true;
        }

        UpdateCornerIndicator();
        SaveAndShareButton.GetComponent<Button>().onClick.AddListener(SaveToOcean);
        SaveAndShareButton.SetActive(false);

        if (scanTutorialBtn != null)
        {
            scanTutorialBtn.onClick.AddListener(tutorialButtonOnClick);
        }
    }

    protected void tutorialButtonOnClick()
    {
        ButtonTool.PausingInteractable(scanTutorialBtn, 1f);
        PrintTutorial();
    }

    protected void PrintTutorial()
    {
        TutorialController.Instance.StartTutorial("PrinterTutorial", GameSetting.PrinterTutorial);
        SceneTransitHandler.LoadScene("Aquarium", true);
    }

    protected void OnTrackingFound(object result)
    {
        isObjectRecognized = true;

        string foundTarget = (string)result;

        if (!hintsAnimator.gameObject.activeSelf)
        {
            hintsAnimator.gameObject.SetActive(true);
        }

        if (string.IsNullOrEmpty(currentTargetName))
        {
            hintsAnimator.CrossFade("StarAnchorToLarge", 0);
        }
        else
        {
            hintsAnimator.CrossFade("StarAnchorLarge", 0);
        }

        if (currentTargetName != foundTarget)
        {
            //Changed target
            currentTargetName = foundTarget;
            changedTarget = true;
        }

        //UI
        SaveAndShareButton.SetActive(false);
    }

    protected void OnTrackingLost(object result)
    {
        isObjectRecognized = false;

        //UI
        UpdateCornerIndicator();
    }

    public void OutOfBound()
    {
        isOutOfBound = true;
    }

    public void ReturnInBounds()
    {
        isOutOfBound = false;

        for (int i = 0; i < cornerIndicator.Length; i++)
        {
            cornerIndicator[i] = false;
        }

        UpdateCornerIndicator();

        SaveAndShareButton.SetActive(true);

        if (changedTarget)
        {
            changedTarget = false;
            modelShowupEffect.SetActive(true);
        }

        hintsAnimator.gameObject.SetActive(false);
    }

    public void OutOfEachBound(bool top, bool bottom, bool left, bool right)
    {
        cornerIndicator[0] = top;
        cornerIndicator[1] = bottom;
        cornerIndicator[2] = left;
        cornerIndicator[3] = right;

        UpdateCornerIndicator();
    }

    public void SaveToCameraRoll()
    {
        StartCoroutine(TakeSnapshotToCameraRoll());
    }

    public void SaveToOcean()
    {
        if (!GameSave.HasSuccessfullyScannedColoringPaper)
        {
            AnalyticsLogCenter.LogEvent("has_scanned_successful");

            GameSave.HasSuccessfullyScannedColoringPaper = true;
        }

        Save();
    }

    public void ShowShare()
    {
        StartCoroutine(TakeSnapshot());
    }

    protected IEnumerator TakeSnapshotToCameraRoll()
    {
        uiContainer.SetActive(false);

        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        texture.Apply();

        uiContainer.SetActive(true);

        Debug.Log("Save snapshot");
        string fileName = "ARAquariumSnapShot.jpg";
        ImageDataWriter.SaveImage(texture, fileName, false);
        ImageDataWriter.SaveImgToGallery(ImageDataWriter.GetSaveFilePath(fileName));
    }

    public void ImgWriteCallback(string result)
    {
        if (result != "success")
        {
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album");
        }

        Debug.Log("ImgWriteCallback: " + result);
    }


    protected IEnumerator TakeSnapshot()
    {
        uiContainer.SetActive(false);

        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, true);
        texture.Apply();

        uiContainer.SetActive(true);

        Hashtable hashtable = new Hashtable();
        hashtable[PopupProperty.Prefab] = PopupManager.Instance.GetPopupPrefab(PopupPrefab.SharePopup);
        hashtable["ScreenShot"] = GeneralTools.TextureToSprite(texture);

        PopupManager.Instance.AddPopupToQueueAndShow(hashtable);
    }

    public void Save()
    {
//		if(!isOutOfBound){

        // ImageTargetController.Instance.CaptureTexture(Back);
        SaveAndShareButton.SetActive(false);
        //ShowMessage(ScanSuccessTxt, 3);

//		}else{
//
//			ShowMessage(OutOfBoundTxt, 1.5f);
//		}
    }

    public void Back()
    {
        if (string.IsNullOrEmpty(DataCenter.Instance.GetSelectedAquarium().NewMember))
        {
            BackToAquarium();
        }
        else
        {
            sceneTansitAnimator.CrossFade("ScanTransition_FadeOut", 0);
        }
    }

    public void BackToAquarium()
    {
        SceneTransitHandler.LoadScene("Aquarium");
    }

    public void DeleteAllSkin() { }

    public void ShowMessage(string text, float time = 2f)
    {
        StartCoroutine(ShowMessageForWhile(text, time));
    }

    IEnumerator ShowMessageForWhile(string text, float time = 2f)
    {
        messageObject.SetActive(true);
        messageBox.text = text;

        yield return new WaitForSeconds(time);
        messageObject.SetActive(false);
    }

    public void UpdateCornerIndicator()
    {
        for (int i = 0; i < cornerIndicatorObject.Length && i < cornerIndicator.Length; i++)
        {
            if (cornerIndicatorObject[i]) cornerIndicatorObject[i].SetActive(isObjectRecognized ? !cornerIndicator[i] : false);
        }
    }
}