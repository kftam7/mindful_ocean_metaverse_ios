using UnityEngine;
using UnityEngine.UI;

public class ScoreEntry : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject penalizeText;

    public void Set(string name, string score, bool leftEarly)
    {
        nameText.text = name;
        scoreText.text = score;
        penalizeText.SetActive(leftEarly);
    }
}