using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class GestureHintPopup : SimplePopup
{
    [SerializeField] private GameObject bar;
    [SerializeField] private GameObject hint;
    [SerializeField] private Image chargingBar;

    public void ShowHint()
    {
        hint.SetActive(true);
        bar.SetActive(false);
    }

    public void ShowBar()
    {
        hint.SetActive(false);
        bar.SetActive(true);
    }

    public void SetBarProgress(float value)
    {
        chargingBar.fillAmount = value;
    }
}