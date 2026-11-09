using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanksMenu : MonoBehaviour
{
    [SerializeField] protected GameObject searchObject;
    [SerializeField] protected Text searchCode;
    [SerializeField] protected Button nextButton;

    // Use this for initialization
    IEnumerator Start()
    {
        nextButton.interactable = false;

        if (!string.IsNullOrEmpty(GameSave.SearchCode))
        {
            searchObject.SetActive(true);
            searchCode.text = GameSave.SearchCode.Substring(0, 4);
        }
        else
        {
            searchObject.SetActive(false);
        }

        yield return new WaitForSeconds(3f);
        nextButton.interactable = true;
    }

    public void Next()
    {
        SceneTransitHandler.LoadScene("FeedbackScene");
    }
}