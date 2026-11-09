using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnswerStatus
{
    Right,
    Wrong,
    Default
}

public class SetCorrect : MonoBehaviour
{
    public GameObject RightImage;
    public GameObject WrongImage;

    public void SetStatus(AnswerStatus status)
    {
        RightImage.SetActive(status == AnswerStatus.Right);
        WrongImage.SetActive(status == AnswerStatus.Wrong);
    }
}