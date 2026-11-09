using System;
using System.Collections;
using System.Globalization;
using Network;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class SubmitCooldownPopup : SimplePopup
{
    [SerializeField] private Text hourText;
    [SerializeField] private Text miniuteText;
    [SerializeField] private Text secondText;

    private void OnEnable()
    {
        StartCoroutine(UpdateTime());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
    }

    public IEnumerator UpdateTime()
    {
        while (true)
        {
            DateTime lastTime = DateTime.ParseExact(GameSave.SubmitArtworkTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            TimeSpan intervalTimeSpan = DateTime.UtcNow - lastTime;
            TimeSpan timeSpan;
            if (NetworkSettings.Instance.Environment == NetworkEnvironmentEnum.Product)
            {
                timeSpan = new TimeSpan(0, 4, 0, 0) - intervalTimeSpan;
            }
            else
            {
                timeSpan = new TimeSpan(0, 0, 0, 60) - intervalTimeSpan;
            }

            int hours = timeSpan.Hours;
            int mins = timeSpan.Minutes;
            int secs = timeSpan.Seconds;

            if (hours < 0 || mins < 0 || secs < 0)
            {
                hours = 0;
                mins = 0;
                secs = 0;
            }

            hourText.text = hours.ToString("00");
            miniuteText.text = mins.ToString("00");
            secondText.text = secs.ToString("00");
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void OnReturnToAquarium()
    {
        Close();
    }
}