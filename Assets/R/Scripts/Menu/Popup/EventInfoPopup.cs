using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class EventInfoPopup : SimplePopup
{
    [SerializeField] private RawImage image;
    [SerializeField] private Text title;
    [SerializeField] private Text timerDescription;
    [SerializeField] private Text timerDays;
    [SerializeField] private Text timerHours;
    [SerializeField] private Text timerMins;
    [SerializeField] private Text paragraphText;

    private EventItemResultData eventData;

    protected override void InitPopup()
    {
        SetEventInfo(ServerCenter.Instance.GetCurrentEventData());
        base.InitPopup();
    }

    public override void Close()
    {
        NativeHelper.GenerateImpactFeedback();
        base.Close();
        if (GameSave.ShowTutorialMainFlow && !DataCenter.Instance.TutorialMainFlowShown)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMainFlowPopup, true);
        }
    }

    public void SetEventInfo(EventItemResultData data)
    {
        if (data == null) return;
        eventData = data;
        title.text = data.GetTile();
        timerDescription.text = data.GetTimerDescription();
        timerDays.text = data.timeSpan.Days.ToString();
        timerHours.text = data.timeSpan.Hours.ToString();
        timerMins.text = data.timeSpan.Minutes.ToString();
        paragraphText.text = data.GetParagraphText();
        ServerCenter.Instance.GetImageFromCacheOrUrl(data.image, true, texture =>
        {
            if (texture != null)
            {
                var copyTexture = new Texture2D(texture.width, texture.height);
                copyTexture.SetPixels(texture.GetPixels());
                copyTexture.Apply();
                image.texture = copyTexture;
            }
        });
    }

    public void VisitOurWebsite()
    {
        Application.OpenURL(eventData.hyperLink);
    }
}