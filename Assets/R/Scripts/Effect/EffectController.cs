using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class EffectController : SceneInstance<EffectController>
{
    public RectTransform UIContainer;
    public const float EffectDepth = -0.32f;


    public void PlayEffect(string prefab, Vector2 position)
    {
        PlayEffect(prefab, new Vector3(position.x, position.y));
    }

    public void PlayEffect(string prefab, Vector3 position)
    {
        GameObject go = ReuseableObjectManager.Instantiate(prefab, this.transform);

        position.z = EffectDepth;
        go.transform.position = position;
    }

    public void PlayHPEffect(string prefab, Color32 color, string content)
    {
        GameObject go = ReuseableObjectManager.Instantiate(prefab, UIContainer);

        Text hp = go.GetComponent<Text>();
        if (hp != null)
        {
            hp.color = color;
            hp.text = content;
        }

        (go.transform as RectTransform).anchoredPosition = new Vector2(0, -60) + Random.insideUnitCircle * 3;
    }
}