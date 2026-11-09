using ProGamer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum TutorialType
{
    Hatchery,
    Metaverse,
    Coloring,
    Preview,
    MetaverseAr,
    DomainAdventure
}

public class TutorialMessagePopup : SimplePopup
{
    [SerializeField] private Text messageText;
    [SerializeField] private Toggle hideTipToggle;
    private TutorialType type;

    public void SetTutorial(TutorialType type)
    {
        this.type = type;
        switch (type)
        {
            case TutorialType.Hatchery:
                messageText.SetLocalization("txt_tut_hatchery_desc");
                break;
            case TutorialType.Coloring:
                messageText.SetLocalization("txt_tut_coloring_desc");
                break;
            case TutorialType.Preview:
                messageText.SetLocalization("txt_tut_preview_desc");
                break;
            case TutorialType.Metaverse:
                messageText.SetLocalization("txt_tut_metaverse_desc");
                break;
            case TutorialType.MetaverseAr:
                messageText.SetLocalization("txt_tut_metaverse_ar_desc");
                break;
            case TutorialType.DomainAdventure:
                messageText.SetLocalization("txt_game_tutorial_tip");
                break;
        }

        hideTipToggle.isOn = !GetTutorialTipState();
    }

    public void ToggleShowTip(bool isOn)
    {
        SetTutorialTipState(isOn);
        NativeHelper.GenerateImpactFeedback();
    }

    private bool GetTutorialTipState()
    {
        switch (type)
        {
            case TutorialType.Hatchery:
                return GameSave.ShowTutorialHatchery;
            case TutorialType.Coloring:
                return GameSave.ShowTutorialColoring;
            case TutorialType.Preview:
                return GameSave.ShowTutorialPreview;
            case TutorialType.Metaverse:
                return GameSave.ShowTutorialMetaverse;
            case TutorialType.MetaverseAr:
                return GameSave.ShowTutorialMetaverseAr;
            case TutorialType.DomainAdventure:
                return GameSave.ShowTutorialDomainAdventureAr;
        }

        return false;
    }

    private void SetTutorialTipState(bool isOn)
    {
        switch (type)
        {
            case TutorialType.Hatchery:
                GameSave.ShowTutorialHatchery = !isOn;
                break;
            case TutorialType.Coloring:
                GameSave.ShowTutorialColoring = !isOn;
                break;
            case TutorialType.Preview:
                GameSave.ShowTutorialPreview = !isOn;
                break;
            case TutorialType.Metaverse:
                GameSave.ShowTutorialMetaverse = !isOn;
                break;
            case TutorialType.MetaverseAr:
                GameSave.ShowTutorialMetaverseAr = !isOn;
                break;
            case TutorialType.DomainAdventure:
                GameSave.ShowTutorialDomainAdventureAr = !isOn;
                break;
        }
    }
}