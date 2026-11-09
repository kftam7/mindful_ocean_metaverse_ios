using System;
using System.Collections;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class HintPopup : SimplePopup
{
    [SerializeField] private GameObject topNode;
    [SerializeField] private Text textTop;
    [SerializeField] private GameObject lowerNode;
    [SerializeField] private Text textLower;


    protected override void OnDisable()
    {
        StopAllCoroutines();
        base.OnDisable();
    }

    public void SetTextKeyTop(string textKey)
    {
        topNode.SetActive(true);
        lowerNode.SetActive(false);
        textTop.SetLocalization(textKey);
    }

    public void SetTextTop(string text)
    {
        topNode.SetActive(true);
        lowerNode.SetActive(false);
        textTop.text = text;
    }

    public void SetTextKeyLower(string textKey)
    {
        topNode.SetActive(false);
        lowerNode.SetActive(true);
        textLower.SetLocalization(textKey);
    }

    public void SetTextLower(string text)
    {
        topNode.SetActive(false);
        lowerNode.SetActive(true);
        textLower.text = text;
    }

    public void SetDelayClose(float delay, Action callback = null)
    {
        StartCoroutine(DelayClose(delay, callback));
    }

    private IEnumerator DelayClose(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        Close();
        callback?.Invoke();
    }
}