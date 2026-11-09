using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class NewVersionPopup : SimplePopup
{
    [SerializeField] private Text version;
    [SerializeField] private Text description;

    protected override void InitPopup()
    {
        base.InitPopup();
        DataCenter.Instance.UpdateVersionPanelShowStats = PanelShowStats.IsShowing;
    }

    public override void Close()
    {
        NativeHelper.GenerateImpactFeedback();
        base.Close();
        DataCenter.Instance.UpdateVersionPanelShowStats = PanelShowStats.Shown;
        if (ServerCenter.Instance.VersionStats != VersionStats.Latest)
        {
            if (GameSave.ShowTutorialMainFlow && !DataCenter.Instance.TutorialMainFlowShown)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMainFlowPopup, true);
            }
        }
    }

    public void SetUpdateInfo(bool force)
    {
        LatestVersionResultData data = ServerCenter.Instance.LatestVersionData;
        version.text = $"V{data.versionNo}";
        if (force)
        {
            Localization.SetText("txt_force_update_info", description);
        }
        else
        {
            description.text = data.description;
        }
    }

    public void OpenAppStore()
    {
        Close();
        Application.OpenURL(GameSetting.Instance.AppStoreUrl);
    }
}