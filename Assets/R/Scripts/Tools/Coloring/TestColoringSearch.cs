using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestColoringSearch : MonoBehaviour
{
    [SerializeField] Color color = Color.white;

    private void test()
    {
        Texture2D target = this.GetComponent<SpriteRenderer>().sprite.texture;

        Texture2D texture = new Texture2D(target.width, target.height);

        Color[] colors = ColoringTools.AutoFillContinuousAlphaRegion(target, 0.1f);

        texture.SetPixels(colors);

        texture.Apply();

        this.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, target.width, target.height), Vector2.one * 0.5f);
    }

    void OnMouseDown()
    {
        test();
    }
}