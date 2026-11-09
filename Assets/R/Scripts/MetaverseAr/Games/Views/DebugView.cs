using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugView : MonoBehaviour
{
    [SerializeField] private Button showDebug, hideDebug;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button clearDate, clearSingleBottle, clearDailyMatch;

    void Start()
    {
        ConfigController.OnConfigFetched += ConfigFetched;
        showDebug.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);

        showDebug.onClick.AddListener(() => panel.SetActive(true));
        hideDebug.onClick.AddListener(() => panel.SetActive(false));
    }

    void OnDestroy()
    {
        ConfigController.OnConfigFetched -= ConfigFetched;
    }

    private void ConfigFetched(GameConfig config)
    {
        if (!config.EnableDebug)
        {
            Destroy(gameObject);
            return;
        }

        showDebug.gameObject.SetActive(true);
        InitButtons();
    }

    void InitButtons()
    {
        clearDate.onClick.AddListener(DecreaseSavedDate);
        clearSingleBottle.onClick.AddListener(ClearSingleBottle);
        clearDailyMatch.onClick.AddListener(ClearDailyMatchCount);
    }

    private void DecreaseSavedDate()
    {
        var dateValue = DateTime.Today.AddDays(-1);
        GameSave.LastSessionDateTime = dateValue;
    }

    private void ClearSingleBottle()
    {
        GameSave.SinglePlayerBottlesCollectedToday = 0;
    }

    private void ClearDailyMatchCount()
    {
        GameSave.MultiPlayerModeMatchCount = 0;
    }
}