using System;
using ProGamer;
using UnityEngine;
using Firebase.Analytics;

public class PurchasePopup : SimplePopup
{
    [SerializeField] private PurchasePopupItem item01;
    [SerializeField] private PurchasePopupItem item02;
    [SerializeField] private PurchasePopupItem item03;
    [SerializeField] private PurchasePopupItem item04;
    [SerializeField] private PurchasePopupItem item05;
    [SerializeField] private PurchasePopupItem item06;
    [SerializeField] private PurchasePopupItem item07;

    private SimplePopup processingPopup;

    void OnEnable()
    {
        Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenIap);
        EventManager.StartListening(EventList.OnPurchaseSuccess, OnPurchaseSuccess);
        EventManager.StartListening(EventList.OnPurchaseFail, OnPurchaseFailed);
    }

    void OnDisable()
    {
        Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenIap);
        EventManager.StopListening(EventList.OnPurchaseSuccess, OnPurchaseSuccess);
        EventManager.StopListening(EventList.OnPurchaseFail, OnPurchaseFailed);
    }

    public override void Close()
    {
        NativeHelper.GenerateImpactFeedback();
        base.Close();
    }

    public void ShowProcessingMask()
    {
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ProcessingPopup, true);
        processingPopup = PopupManager.Instance.ActivePopup;
    }

    public void ShowBandItems()
    {
        item01.gameObject.SetActive(false);
        item02.gameObject.SetActive(false);
        item03.gameObject.SetActive(false);
        item04.gameObject.SetActive(false);
        item05.gameObject.SetActive(false);
        item06.gameObject.SetActive(false);
        item07.gameObject.SetActive(false);

        int bandStatus = DataCenter.Instance.GetBandPurchaseStatus();
        if (bandStatus == 0)
        {
            item01.gameObject.SetActive(true);
            item02.gameObject.SetActive(true);
            item03.gameObject.SetActive(true);
            item07.gameObject.SetActive(false);
        }
        else
        {
            if (bandStatus == 1)
            {
                item04.gameObject.SetActive(true);
                item06.gameObject.SetActive(true);
            }
            else if (bandStatus == 2)
            {
                item05.gameObject.SetActive(true);
            }

            item07.gameObject.SetActive(!DataCenter.Instance.IsAccessoryPurchased());
        }
    }

    public void ShowAccessoryItem()
    {
        item01.gameObject.SetActive(false);
        item02.gameObject.SetActive(false);
        item03.gameObject.SetActive(false);
        item04.gameObject.SetActive(false);
        item05.gameObject.SetActive(false);
        item06.gameObject.SetActive(false);
        item07.gameObject.SetActive(true);
    }

    public Action PurchaseSuccessCallback;

    private void OnPurchaseSuccess(object result)
    {
        string packageName = result.ToString().Split(".")[^1];
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase,
            FirebaseAnalytics.ParameterItemId,
            packageName);
        Debug.Log($"[{GetType().Name}] Purchase success: {result}");
        processingPopup.Close();
        PurchaseSuccessCallback?.Invoke();
        Close();
    }

    void OnPurchaseFailed(object result)
    {
        string failReason = (string)result;
        Debug.Log($"[{GetType().Name}] Purchase fail: {failReason}");
        processingPopup.Close();
        PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_sorry_fail", null, true);
    }
}