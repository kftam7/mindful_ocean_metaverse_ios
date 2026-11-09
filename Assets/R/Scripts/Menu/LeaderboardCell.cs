using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class LeaderboardCell : MonoBehaviour
{
    public Image OrderImage;
    public Image ModelIcon;
    public Text PlayerName;
    public Text ScoreText;
    public Image Bg;
    public GameObject[] OrderIndicator;

    public Sprite[] ModelIcons;
    protected string[] modelName = new string[] { "Dolphin", "Shark", "Tuna", "Turtle" };

    public void SetUI(Sprite modelIcon, string playerName, int score, int order, bool isCurrentPlayer)
    {
        this.ModelIcon.sprite = modelIcon;
        this.PlayerName.text = playerName;
        this.ScoreText.text = score.ToString();
        this.PlayerName.color = isCurrentPlayer ? new Color32(194, 244, 251, 255) : new Color32(255, 231, 97, 255);
        this.Bg.color = isCurrentPlayer ? new Color32(63, 207, 216, 255) : new Color32(46, 94, 97, 255);

        GameObject indicator = OrderIndicator[Mathf.Min(OrderIndicator.Length - 1, order)];
        indicator.SetActive(true);
        indicator.GetComponentInChildren<Text>().text = (order + 1).ToString();
    }

    public void SetUI(LeaderboardElement data, int order, bool isCurrentPlayer)
    {
        SetUI(GetModelSprite(data.ModelName), isCurrentPlayer ? Localization.Get("txt_you") : "**********", data.Score, order, isCurrentPlayer);
    }

    public Sprite GetModelSprite(string name)
    {
        for (int i = 0; i < ModelIcons.Length; i++)
        {
            if (name == modelName[i])
            {
                return ModelIcons[i];
            }
        }

        return null;
    }
}