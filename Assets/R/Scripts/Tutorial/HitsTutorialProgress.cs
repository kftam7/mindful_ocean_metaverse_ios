using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitsTutorialProgress : IndicatorTutorialProgress
{
    public override GameObject GetIndicator(string prefab)
    {
        GameObject go = base.GetIndicator(prefab);

        message = go.GetComponentInChildren<Text>();

        return go;
    }
}