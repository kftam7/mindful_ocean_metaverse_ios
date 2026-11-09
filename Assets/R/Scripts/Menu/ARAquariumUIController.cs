using System;
using System.Collections;
using Auki.ConjureKit;
using DG.Tweening;
using ProGamer;
using ProGamer.UIAnimation;
using UnityEngine;
using UnityEngine.UI;

public class ARAquariumUIController : SceneInstance<ARAquariumUIController>
{
    [SerializeField] private Image joinSpaceFader;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject connectButton;
    [SerializeField] private GameObject domainButton;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private DOTweenAnimation sessionTextPanel;
    [SerializeField] private Text sessionText;
    [SerializeField] private Button gestureButton;
    [SerializeField] private GameObject gestureSelector;
    [SerializeField] private GameObject[] gestures;
    public UITweenFadeCrossAnimation MenuTransition;

    [Header("Scanner Component")]
    [SerializeField] private GameObject scanner;
    [SerializeField] private Button scannerCloseButton;
    [SerializeField] private Button scannerSkipButton;

    private bool IsHatchery => GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId;

    protected bool isMenuShown = true;
    private bool _isInDomain;

    private Tween _sessionTextTween;

    /// <summary>
    /// event to hook leaving the game mode
    /// </summary>
    public static event Action OnLeaveGame;

    private void Awake()
    {
        if (connectButton != null)
        {
            connectButton.SetActive(false);
        }

        if (domainButton != null)
        {
            domainButton.SetActive(false);
        }
    }

    private IEnumerator Start()
    {
        if (IsHatchery)
        {
            Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenHatcheryAr);
            sessionTextPanel.gameObject.SetActive(false);
        }
        else
        {
            sessionTextPanel.gameObject.SetActive(true);
            _sessionTextTween = sessionTextPanel.GetTweens()[0];
            _sessionTextTween.Complete();

            Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenMetaverseAr);
            AukiSdkWrapper.Instance.JoinedEvent += OnJoined;
            AukiSdkWrapper.Instance.IsLoading += ShowLoadingPanel;
            AukiSdkWrapper.Instance.LeftEvent += LeftSession;

            if (GameSave.ShowTutorialMetaverseAr)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMessagePopup, true);
                var popup = PopupManager.Instance.ActivePopup as TutorialMessagePopup;
                popup.SetTutorial(TutorialType.MetaverseAr);
            }

            while (!AukiSdkWrapper.Instance.HasJoined)
            {
                yield return null;
            }

            connectButton.SetActive(true);
            domainButton.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (IsHatchery)
        {
            Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenHatcheryAr);
        }
        else
        {
            Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenMetaverseAr);
            AukiSdkWrapper.Instance.JoinedEvent -= OnJoined;
            AukiSdkWrapper.instance.IsLoading -= ShowLoadingPanel;
        }
    }

    public void MenuFadeIn()
    {
        if (!isMenuShown)
        {
            MenuTransition.FadeIn();
            isMenuShown = true;
        }
    }

    public void MenuFadeOut()
    {
        if (isMenuShown)
        {
            MenuTransition.FadeOut();
            isMenuShown = false;
        }
    }

    public void Back()
    {
        NativeHelper.GenerateImpactFeedback();
        if (AukiSdkWrapper.Instance.IsInDomain)
        {
            if (OnLeaveGame != null)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.ConfirmPopup, true);
                var popup = PopupManager.Instance.ActivePopup as GeneralReusablePopup;
                var param = new Hashtable
                {
                    { PopupProperty.Title, "txt_leaving_domain" },
                    { PopupProperty.ConfirmButton, new ButtonFormat(() => OnLeaveGame?.Invoke()) },
                    { PopupProperty.DismissButton, new ButtonFormat("") }
                };
                popup.SetParameters(param);
                popup.Show();
                return;
            }

            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.ConfirmPopup, true);
                var popup = PopupManager.Instance.ActivePopup as GeneralReusablePopup;
                var param = new Hashtable
                {
                    { PopupProperty.Title, "txt_leaving_domain" },
                    { PopupProperty.ConfirmButton, new ButtonFormat(() => AukiSdkWrapper.Instance.LeaveDomain()) },
                    { PopupProperty.DismissButton, new ButtonFormat("") }
                };
                popup.SetParameters(param);
                popup.Show();
                return;
            }
        }

        if (!IsHatchery)
        {
            ARMetaverseManager.Instance.DeleteMyFishFromServer();
            ARMetaverseManager.Instance.DeleteMyMagicObjectsFromServer();
        }

        StartCoroutine(DelayLoadScene());
    }

    private IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneTransitHandler.LoadScene("Aquarium");
    }

    public void ShowJoinSpaceFader()
    {
        joinSpaceFader.gameObject.SetActive(true);
        joinSpaceFader.DOColor(Color.white, 1.5f)
            .Play()
            .SetAutoKill(true)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => joinSpaceFader.gameObject.SetActive(false));
    }

    public void ShowScanner()
    {
        backButton.SetActive(false);
        connectButton.SetActive(false);
        domainButton.SetActive(false);
        gestureButton.gameObject.SetActive(false);
        scanner.SetActive(true);
        scanner.GetComponentInChildren<DOTweenAnimation>().DORestart();

        scannerCloseButton.onClick.AddListener(() =>
        {
            HideScanner();
            scannerCloseButton.onClick.RemoveAllListeners();
        });

        scannerSkipButton.gameObject.SetActive(false);
        AukiSdkWrapper.Instance.EnableScanner(true);
    }

    public void ShowScannerForDomain(Action onSkipped, Action onClose = null)
    {
        backButton.SetActive(false);
        connectButton.SetActive(false);
        domainButton.SetActive(false);
        gestureButton.gameObject.SetActive(false);
        scanner.SetActive(true);
        scanner.GetComponentInChildren<DOTweenAnimation>().DORestart();
        scannerSkipButton.gameObject.SetActive(true);

        scannerCloseButton.onClick.AddListener(() =>
        {
            onClose?.Invoke();
            HideScanner();
            scannerCloseButton.onClick.RemoveAllListeners();
        });

        scannerSkipButton.onClick.AddListener(() =>
        {
            onSkipped?.Invoke();
            HideScanner();
            scannerSkipButton.onClick.RemoveAllListeners();
        });

        AukiSdkWrapper.Instance.EnableScanner(true);
    }

    public void FreezeScanner()
    {
        scanner.GetComponentInChildren<DOTweenAnimation>().DOPause();
    }

    public void HideScanner()
    {
        backButton.SetActive(true);
        connectButton.SetActive(true);
        domainButton.SetActive(!_isInDomain);
        gestureButton.gameObject.SetActive(true);
        scanner.SetActive(false);
        AukiSdkWrapper.Instance.EnableScanner(false);
    }

    public void EnableAdventureButton(bool enable)
    {
        _isInDomain = !enable;
        domainButton.SetActive(enable);
    }

    public void ShowGesture(int index)
    {
        gestureSelector.SetActive(false);
        foreach (GameObject gesture in gestures)
        {
            gesture.SetActive(false);
        }

        StartCoroutine(ShowGestureForAWhile(index));
    }

    public void ShowLoadingPanel(bool show)
    {
        loadingScreen.SetActive(show);
    }

    private IEnumerator ShowGestureForAWhile(int index)
    {
        gestures[index].SetActive(true);
        gestureButton.interactable = false;
        yield return new WaitForSeconds(3);
        gestures[index].SetActive(false);
        gestureButton.interactable = true;
    }

    private void OnJoined(Session obj)
    {
        HideScanner();
        _sessionTextTween.PlayBackwards();
        sessionText.text = $"Session ID: {obj.Id}";
    }

    private void LeftSession()
    {
        _sessionTextTween.Restart();
        sessionText.text = string.Empty;
    }
}