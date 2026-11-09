using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class PrintingProblemPopup : ProGamer.SimplePopup
{
    [SerializeField] Button[] ReasonButton;
    [SerializeField] string[] Reasons;
    [SerializeField] Text Question;

    protected override void InitPopup()
    {
        base.InitPopup();

        for (int i = 0; i < ReasonButton.Length; i++)
        {
            int c = i;
            ReasonButton[i].onClick.RemoveAllListeners();
            ReasonButton[i].onClick.AddListener(() =>
            {
                OnOptionSelected(c);
            });
        }

        for (int r = 0; r < ReasonButton.Length && r < Reasons.Length; r++)
        {
            ReasonButton[r].GetComponentInChildren<UILocalize>().Key = Reasons[r];
        }

        Question.text = ProGamer.Localization.Get(GameSave.IsCollectedPrintingProblemReason ? "txt_printing_prompt_again_title" : "txt_printing_prompt_title");
        Localization.SetFont(Question);
    }

    public void OnOptionSelected(int i)
    {
        if (i < Reasons.Length)
        {
            Debug.Log("Selected: " + Reasons[i]);

            if (!GameSave.IsCollectedPrintingProblemReason)
            {
                AnalyticsLogCenter.LogEvent("tutorial_printing_problem_prompt", new Dictionary<string, object>()
                {
                    { "reason", Reasons[i] }
                });

                GameSave.FirstPrintFailTime = System.DateTime.UtcNow.ToString();
                GameSave.IsCollectedPrintingProblemReason = true;
            }
            else
            {
                AnalyticsLogCenter.LogEvent("day1_after_no_scan", new Dictionary<string, object>()
                {
                    { "reason", Reasons[i] }
                });

                GameSave.FirstPrintFailTime = "";
            }
        }

        Close();
    }
}