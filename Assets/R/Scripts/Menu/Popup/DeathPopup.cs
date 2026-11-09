using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathPopup : ProGamer.SimplePopup
{
    [SerializeField] protected Image deathImage;
    [SerializeField] protected Text deathTitle;
    [SerializeField] protected Text deathDescription;

    [SerializeField] protected Sprite[] deathImageSprites;

    protected override void InitPopup()
    {
        base.InitPopup();

        DeadReason reason = (DeadReason)this.parameters["DeadReason"];

        deathImage.sprite = deathImageSprites[(int)reason];

        if (reason == DeadReason.Starve)
        {
            deathTitle.text = ProGamer.Localization.Get("txt_death_reason_starvation");
            deathDescription.text = ProGamer.Localization.Get("txt_death_desc_starvation");
        }
        else
        {
            deathTitle.text = ProGamer.Localization.Get("txt_death_reason_plastic");
            deathDescription.text = ProGamer.Localization.Get("txt_death_desc_plastic");
        }
    }

    public void ShowResult()
    {
        base.Close();
        ProGamer.PopupManager.Instance.QuickShowQueuePopup(PopupPrefab.ResultsPopup);
    }
}