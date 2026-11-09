using System;
using ProGamer;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisclaimerPopup : SimplePopup, IPointerClickHandler
{
    [SerializeField] private TextAsset termEn01;
    [SerializeField] private TextAsset termEn02;
    [SerializeField] private TextMeshProUGUI contentText01;
    [SerializeField] private TextMeshProUGUI contentText02;
    private SimplePopup processingPopup;
    private Camera renderCamera;
    public Action OnSuccess;

    protected override void InitPopup()
    {
        contentText01.text = termEn01.text;
        contentText02.text = termEn02.text;
        base.InitPopup();
        Canvas canvas = GetComponentInParent<Canvas>();
        renderCamera = canvas.worldCamera;
    }

    public void OnPressConfirm()
    {
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ProcessingPopup, true);
        processingPopup = PopupManager.Instance.ActivePopup;
        ServerCenter.Instance.AcceptTerms(
            _ =>
            {
                GameSave.TermAccepted = true;
                processingPopup.Close();
                Close();
                OnSuccess?.Invoke();
            }, () =>
            {
                processingPopup.Close();
                PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_sorry_fail", null, true);
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // First, get the index of the link clicked. Each of the links in the text has its own index.
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(contentText01, Input.mousePosition, renderCamera);
        if (linkIndex < 0) return;

        // As the order of the links can vary easily (e.g. because of multi-language support),
        // you need to get the ID assigned to the links instead of using the index as a base for our decisions.
        // you need the LinkInfo array from the textInfo member of the TextMesh Pro object for that.
        var linkId = contentText01.textInfo.linkInfo[linkIndex].GetLinkID();

        // Now finally you have the ID in hand to decide what to do. Don't forget,
        // you don't need to make it act like an actual link, instead of opening a web page,
        // any kind of functions can be called.
        var url = linkId switch
        {
            "privacy_policy" => GameSetting.Instance.PrivacyPolicyUrl,
            _ => GameSetting.Instance.PrivacyPolicyUrl
        };

        Debug.Log($"[{GetType().Name}] URL clicked: linkInfo[{linkIndex}].id={linkId} ==> url={url}");
        Application.OpenURL(url);
    }
}