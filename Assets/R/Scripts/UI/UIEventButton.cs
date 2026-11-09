using System;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class UIEventButton : MonoBehaviour
{
    [SerializeField] private GameObject note;
    [SerializeField] private Text noteText;
    [SerializeField] private GameObject activeSubmitArrow;
    [SerializeField] private GameObject comingSubmitArrow;

    private EventStatus status;
    private int leftDays;
    private bool showSubmitButton;

    private void OnEnable()
    {
        Localization.instance.LocalizeUpdate += OnLocalize;
    }

    private void OnDisable()
    {
        Localization.instance.LocalizeUpdate -= OnLocalize;
    }

    public void SetCanSubmit(bool value)
    {
        showSubmitButton = value;
        if (!showSubmitButton 
            || status is EventStatus.Inactive or EventStatus.Ended
            || ServerCenter.Instance.VersionStats != VersionStats.Latest)
        {
            activeSubmitArrow.SetActive(false);
            comingSubmitArrow.SetActive(false);
        }
        else if (status == EventStatus.Active)
        {
            activeSubmitArrow.SetActive(true);
            comingSubmitArrow.SetActive(false);
        }
        else
        {
            activeSubmitArrow.SetActive(false);
            comingSubmitArrow.SetActive(true);
        }
    }

    public void SetEvent(EventStatus status, int leftDays)
    {
        this.status = status;
        this.leftDays = leftDays;
        if (status is EventStatus.Inactive or EventStatus.Ended)
        {
            gameObject.SetActive(false);
        }
        else if (status == EventStatus.Active)
        {
            gameObject.SetActive(true);
            activeSubmitArrow.SetActive(showSubmitButton);
            comingSubmitArrow.SetActive(false);
            note.SetActive(true);
            if (leftDays >= 1)
            {
                noteText.text = string.Format(Localization.Get("txt_left_days"), leftDays);
            }
            else
            {
                noteText.text = Localization.Get("txt_end_soon");
            }
        }
        else
        {
            gameObject.SetActive(true);
            activeSubmitArrow.SetActive(false);
            comingSubmitArrow.SetActive(showSubmitButton);
            note.SetActive(true);
            if (leftDays >= 7)
            {
                noteText.SetLocalization("txt_coming");
            }
            else if (leftDays >= 1)
            {
                noteText.text = string.Format(Localization.Get("txt_start_in_days"), leftDays + 1);
            }
            else
            {
                noteText.SetLocalization("txt_start_soon");
            }
        }
    }

    private void OnLocalize()
    {
        SetEvent(status, leftDays);
    }
}