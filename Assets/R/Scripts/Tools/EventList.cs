using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventList
{
    public const string OnHpPercentageUpdated = "OnHpPercentageUpdated";

    public const string OnGameStatusChanged = "OnGameStatusChanged";
    public const string OnGotFood = "OnGotFood";
    public const string OnScoreUpdated = "OnScoreUpdated";

    public const string OnCoinUpdated = "OnCoinUpdated";
    public const string OnTimeUpdated = "OnTimeUpdated";

    public const string OnTrackingFound = "OnTrackingFound";
    public const string OnTrackingLost = "OnTrackingLost";

    public const string OnTrackingOutOfBounds = "OnTrackingOutOfBounds";
    public const string OnTrackingReturnInBounds = "OnTrackingReturnInBounds";

    public const string OnAquariumPlacement = "OnAquariumPlacement";

    public const string OnARModeActivated = "OnARModeActivated";

    public const string OnTaskProgressUpdated = "OnTaskProgressUpdated";
    public const string OnSkinEditModeStatusChanged = "OnSkinEditModeStatusChanged";
    public const string OnPrintDone = "OnPrintDone";

    public const string OnSkinDeleted = "OnSkinDeleted";

    public const string OnModelPurchased = "OnModelPurchased";

    //IAP
    public const string OnPurchaseSuccess = "OnPurchaseSuccess";
    public const string OnPurchaseFail = "OnPurchaseFail";

    public const string OnPurchaseRestoreDone = "OnPurchaseRestoreDone";
}