using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using ProGamer;

public class Pages : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] List<Sprite> pagesSprite;
    [SerializeField] List<Image> pageIndicator;
    [SerializeField] List<string> pageDescription;
    [SerializeField] Color32 indicatorOn = new Color32(255, 255, 255, 255);
    [SerializeField] Color32 indicatorOff = new Color32(0, 0, 0, 255);
    [SerializeField] GameObject LeftArrow;
    [SerializeField] GameObject RightArrow;
    [SerializeField] UnityEvent OnPageReachedStart;
    [SerializeField] UnityEvent OnPageReachedEnd;

    protected int currentPages = 0;

    public Image PageView;
    public Text Description;

    // Use this for initialization
    void Start()
    {
        SetPage(currentPages);
    }

    public void SetPage(int index)
    {
        PageView.sprite = pagesSprite[index];
        Description.text = ProGamer.Localization.Get(pageDescription[index]);
        Localization.SetFont(Description);
        for (int i = 0; i < pageIndicator.Count; i++)
        {
            pageIndicator[i].color = i == index ? indicatorOn : indicatorOff;
        }

        if (LeftArrow)
        {
            LeftArrow.SetActive(currentPages != 0);
        }

        if (RightArrow)
        {
            RightArrow.SetActive(currentPages != pagesSprite.Count - 1);
        }

        if (currentPages == 0 && OnPageReachedStart != null)
        {
            OnPageReachedStart.Invoke();
        }

        if (currentPages == pagesSprite.Count - 1 && OnPageReachedEnd != null)
        {
            OnPageReachedEnd.Invoke();
        }
    }

    public void NextPage()
    {
        currentPages = Mathf.Min(currentPages + 1, pagesSprite.Count - 1);
        SetPage(currentPages);
    }

    public void PerviousPage()
    {
        currentPages = Mathf.Max(currentPages - 1, 0);
        SetPage(currentPages);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pressPosition.x >= Screen.width / 2f)
            NextPage();
        else
            PerviousPage();
    }
}