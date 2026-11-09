using System;
using System.Linq;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;
using ProGamer;
using UnityEngine.UI;

public class AquariumManageUI : SceneInstance<AquariumManageUI>
{
    [SerializeField] protected GameObject HomeMenu;
    [SerializeField] protected GameObject Title;
    [SerializeField] protected UIEventButton EventButton;
    [SerializeField] protected GameObject TransitionEffect;
    [SerializeField] protected GameObject ScrollViewGO;
    [SerializeField] protected InfiniteScrollView infiniteScrollView;

    [SerializeField] private GameObject signInGo;

    // expand on start
    [SerializeField] private DOTweenAnimation signInButtonAnim;

    [SerializeField] private GameObject signInInfoGo;

    // collapse on start
    [SerializeField] private DOTweenAnimation signInInfoAnim;
    [SerializeField] private Text signInInfoNameText;

    private bool signInButtonIsExpanded = true;
    private bool signInInfoIsExpanded;

    private void Start()
    {
        Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenMain);
        InitAquariumStore();
        EventButton.SetCanSubmit(false);
        EventButton.SetEvent(EventStatus.Inactive, 0);

        SetSignInButtons();
        if (GameSave.LastSelectedAquariumCellIndex == 1)
        {
            signInButtonAnim.DOComplete();
            signInButtonIsExpanded = false;
        }

        InvokeRepeating(nameof(UpdateVersion), 0.1f, 5f);
        InvokeRepeating(nameof(UpdateEvents), 0.1f, 5f);
    }

    private void OnEnable()
    {
        ServerCenter.Instance.EventDataUpdatedEvent += OnEventDataUpdated;
        ServerCenter.Instance.SignedInEvent += OnSignedIn;
        ServerCenter.Instance.UserInfoUpdatedEvent += OnUserInfoUpdated;
        GlobalEvents.MainUiScrollToEvent += OnMainUiScrollTo;
    }

    private void OnDisable()
    {
        ServerCenter.Instance.EventDataUpdatedEvent -= OnEventDataUpdated;
        ServerCenter.Instance.SignedInEvent -= OnSignedIn;
        ServerCenter.Instance.UserInfoUpdatedEvent -= OnUserInfoUpdated;
        GlobalEvents.MainUiScrollToEvent -= OnMainUiScrollTo;
    }

    private void OnDestroy()
    {
        Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenMain);
    }

    public void ShowARAquarium()
    {
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        TransitionEffect.SetActive(true);
        StartCoroutine(TransitScene("Aquarium"));
    }

    public void ShowSetting()
    {
        NativeHelper.GenerateImpactFeedback();
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");
        SetHomeActive(false);
        PopupManager.Instance.QuickShowPopup(PopupPrefab.SettingPopup);
    }

    public void SetHomeActive(bool isactive)
    {
        HomeMenu.GetComponent<CanvasGroup>().alpha = isactive ? 1 : 0;

        if (Title != null)
        {
            Title.SetActive(isactive);
        }

        if (ScrollViewGO != null)
        {
            ScrollViewGO.SetActive(isactive);
        }

        signInGo.SetActive(!ServerCenter.Instance.IsLoggedIn && isactive);
        signInInfoGo.SetActive(ServerCenter.Instance.IsLoggedIn && isactive);
    }

    public void ShowEventInfo()
    {
        NativeHelper.GenerateImpactFeedback();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.EventInfoPopup, true);
    }

    public void ShowSignInWithApplePopup()
    {
        PopupManager.Instance.QuickShowPopup(PopupPrefab.SignInWithApplePopup, true);
    }

    public void InitAquariumStore()
    {
        infiniteScrollView.UpdateData(DataCenter.Instance.AquariumsData.Values.ToList());
        infiniteScrollView.ScrollTo(GameSave.LastSelectedAquariumCellIndex);
    }

    public void SwitchSignInInfoStatus()
    {
        if (signInInfoIsExpanded)
        {
            signInInfoAnim.DOPlayBackwards();
            signInInfoIsExpanded = false;
        }
        else
        {
            signInInfoAnim.DOPlayForward();
            signInInfoIsExpanded = true;
        }
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
        ServerCenter.Instance.UpdateEvents(null, () => // update events failed
        {
            if (GameSave.ShowTutorialMainFlow && !DataCenter.Instance.TutorialMainFlowShown)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMainFlowPopup, true);
            }
        });
    }

    private IEnumerator TransitScene(string scene)
    {
        if (EventSystem.current != null && EventSystem.current.gameObject != null)
        {
            EventSystem.current.gameObject.SetActive(false);
            Debug.Log("Event System set to false");
        }

        // Wait 2.5 seconds
        yield return new WaitForSeconds(2.5f);
        if (GameSave.SelectedAquarium == GameSetting.MetaverseAquariumId)
        {
            ImageDataManager.Instance.UpdateServerImageData(_ =>
            {
                SceneTransitHandler.LoadScene(scene);
            });
        }
        else
        {
            SceneTransitHandler.LoadScene(scene);
        }
    }

    private void SetSignInButtons()
    {
        if (ServerCenter.Instance.IsLoggedIn)
        {
            signInGo.SetActive(false);
            if (!string.IsNullOrWhiteSpace(ServerCenter.Instance.AppleName))
            {
                signInInfoGo.SetActive(true);
                signInInfoNameText.text = ServerCenter.Instance.AppleName;
            }
        }
        else
        {
            signInGo.SetActive(true);
            signInInfoGo.SetActive(false);
        }
    }

    private IEnumerator PlayCollapseSignInInfoGo()
    {
        if (!signInButtonIsExpanded)
        {
            signInInfoAnim.DOPlayForward();
            signInInfoIsExpanded = true;
            yield return new WaitForSeconds(signInInfoAnim.duration + 0.2f);
        }
        else
        {
            signInInfoAnim.DOComplete();
            signInInfoIsExpanded = true;
        }

        yield return new WaitForSeconds(1);
        if (signInInfoIsExpanded)
        {
            signInInfoAnim.DOPlayBackwards();
            signInInfoIsExpanded = false;
        }
    }

    /// <summary>
    /// Setup event button and event info panel.
    /// </summary>
    private void OnEventDataUpdated()
    {
        EventItemResultData data = ServerCenter.Instance.GetCurrentEventData();
        int eventId = data?.eventId ?? -1;
        if (eventId != DataCenter.Instance.CurrentEventId)
        {
            ImageDataManager.Instance.FillImageDataLists(true);
            DataCenter.Instance.CurrentEventId = eventId;
        }
        
        if (data == null)
        {
            EventButton.SetEvent(EventStatus.Inactive, 0);
            return;
        }

        EventButton.SetEvent(data.GetStatus(), data.timeSpan.Days);
        ServerCenter.Instance.GetImageFromCacheOrUrl(data.image, true, _ =>
        {
            if (!DataCenter.Instance.EventInfoShown
                && ServerCenter.Instance.VersionStats == VersionStats.Latest)
            {
                ShowEventInfo();
                DataCenter.Instance.EventInfoShown = true;
            }
        });
    }

    private void OnSignedIn(string jsonData)
    {
        SetSignInButtons();
        StartCoroutine(PlayCollapseSignInInfoGo());
    }

    private void OnUserInfoUpdated()
    {
        signInInfoGo.SetActive(true);
        signInInfoNameText.text = ServerCenter.Instance.AppleName;
    }

    private void OnMainUiScrollTo(int index)
    {
        if (index == 0)
        {
            signInButtonAnim.DOPlayBackwards();
            signInButtonIsExpanded = true;
        }
        else if (index == 1)
        {
            signInButtonAnim.DOPlayForward();
            signInButtonIsExpanded = false;
        }
    }
}