using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PatinCraft.UI;

public class BrushSizeToggle : MonoBehaviour
{
    public GameObject Target;

    private void OnEnable()
    {
        ChangeBrushOnClickController.OnBrushChanged.AddListener(OnBrushChanged);
        OnBrushChanged(PlayerPrefs.GetString("LineConfig"));
    }

    private void OnDisable()
    {
        ChangeBrushOnClickController.OnBrushChanged.RemoveListener(OnBrushChanged);
    }

    void OnBrushChanged(string brushName)
    {
        if (Target) Target.SetActive(!brushName.Contains("Bucket"));
    }
}