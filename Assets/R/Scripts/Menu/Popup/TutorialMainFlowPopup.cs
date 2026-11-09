using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMainFlowPopup : SimplePopup
{
    [SerializeField] private Toggle showTipToggle;

    private void OnEnable()
    {
        showTipToggle.isOn = !GameSave.ShowTutorialMainFlow;
        DataCenter.Instance.TutorialMainFlowShown = true;
    }

    public void ToggleShowTip(bool isOn)
    {
        GameSave.ShowTutorialMainFlow = !isOn;
        NativeHelper.GenerateImpactFeedback();
    }
}