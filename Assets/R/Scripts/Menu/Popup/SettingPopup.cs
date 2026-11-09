using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class SettingPopup : SimplePopup
{
    [SerializeField] protected Toggle musicToggle;
    [SerializeField] protected Toggle sfxToggle;
    [SerializeField] protected GameObject printTutorialButton;
    [SerializeField] protected Toggle showTipsToggle;

    //Prevent toggle callback
    protected bool initReady = false;

    protected override void InitPopup()
    {
        base.InitPopup();

        initReady = false;
        musicToggle.isOn = GameSave.MusicOn;
        sfxToggle.isOn = GameSave.SfxOn;
        showTipsToggle.isOn = GameSave.ShowTutorialMainFlow
                              && GameSave.ShowTutorialHatchery
                              && GameSave.ShowTutorialColoring
                              && GameSave.ShowTutorialPreview
                              && GameSave.ShowTutorialMetaverse
                              && GameSave.ShowTutorialMetaverseAr
                              && GameSave.ShowTutorialDomainAdventureAr;

        initReady = true;

        printTutorialButton.SetActive(false); //Application.isEditor);
    }

    public void SetLanague(string lan)
    {
        if (initReady)
        {
            Localization.language = lan;
            NativeHelper.GenerateImpactFeedback();
        }
    }

    public override void Close()
    {
        AquariumManageUI.Instance.SetHomeActive(true);
        NativeHelper.GenerateImpactFeedback();
        base.Close();
    }

    public void Reset()
    {
        AnalyticsLogCenter.LogEvent("replay_tutorial", new Dictionary<string, object>()
        {
            { "tutorial_name", "BeginnerTutorial" }
        });

        TutorialController.Instance.StartTutorial("BeginnerTutorial", GameSetting.BeginnerTutorial);
        SceneTransitHandler.LoadScene("Aquarium", true);
    }

    public void PrintTutorial()
    {
        TutorialController.Instance.StartTutorial("PrinterTutorial", GameSetting.PrinterTutorial);
        SceneTransitHandler.LoadScene("Aquarium", true);
    }

    public void ToggleBGM(bool isOn)
    {
        if (initReady)
        {
            SoundManager.Instance.ToggleMusic(isOn);
            NativeHelper.GenerateImpactFeedback();
        }
    }

    public void ToggleSFX(bool isOn)
    {
        if (initReady)
        {
            SoundManager.Instance.ToggleSFX(isOn);
            NativeHelper.GenerateImpactFeedback();
        }
    }

    public void ToggleShowTips(bool isOn)
    {
        if (initReady && isOn)
        {
            GameSave.ShowTutorialColoring = true;
            GameSave.ShowTutorialHatchery = true;
            GameSave.ShowTutorialMetaverse = true;
            GameSave.ShowTutorialPreview = true;
            GameSave.ShowTutorialMainFlow = true;
            GameSave.ShowTutorialMetaverseAr = true;
            GameSave.ShowTutorialDomainAdventureAr = true;
            NativeHelper.GenerateImpactFeedback();
        }
    }

    public void ToggleNotification(bool isOn)
    {
        if (initReady)
        {
            NativeHelper.GenerateImpactFeedback();
        }
    }

    public void MoreGames() { }

    public void ShowPrivacyPolicy()
    {
//		ProGamer.PopupManager.Instance.QuickShowPopupWebview("https://www.outblaze.com/volvoar-privacy-policy");
        Application.OpenURL(GameSetting.Instance.PrivacyPolicy);
        NativeHelper.GenerateImpactFeedback();
    }

    public void ShowMoreInfo()
    {
        Application.OpenURL(GameSetting.Instance.GameWebSite);
        NativeHelper.GenerateImpactFeedback();
    }

    public void ShowCustomerSupport()
    {
        Application.OpenURL("mailto:" + GameSetting.Instance.CustomerSupport);
        NativeHelper.GenerateImpactFeedback();
    }

    public void RestorePurchase()
    {
        Purchaser.Instance.RestorePurchases();
        NativeHelper.GenerateImpactFeedback();
    }

    public void GotoTutorial(int index)
    {
        if (index == 0)
        {
            FirebaseAnalytics.LogEvent(AnalyticsEnums.EventClickTutorialHatchery);
            Application.OpenURL(GameSetting.Instance.TutorialHatcheryUrl);
        }
        else if (index == 1)
        {
            FirebaseAnalytics.LogEvent(AnalyticsEnums.EventClickTutorialMetaverse);
            Application.OpenURL(GameSetting.Instance.TutorialMetaverseUrl);
        }
        else if (index == 2)
        {
            FirebaseAnalytics.LogEvent(AnalyticsEnums.EventClickTutorialFaq);
            Application.OpenURL(GameSetting.Instance.GameWebSiteFaq);
        }

        NativeHelper.GenerateImpactFeedback();
    }
}