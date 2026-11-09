using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer.UIAnimation;

public class PageSwitcher : MonoBehaviour
{
    [SerializeField] int currentIndex = 0;

    [SerializeField] UITweenFadeCrossAnimation[] pages;

    public void SwitchPage(int pageIndex)
    {
        if (currentIndex != pageIndex)
        {
            if (!pages[pageIndex].gameObject.activeSelf)
            {
                pages[pageIndex].gameObject.SetActive(true);
            }

            pages[pageIndex].FadeIn();
            pages[currentIndex].FadeOut();

            currentIndex = pageIndex;
        }
    }

    public void NextPage()
    {
        int target = Mathf.Min(pages.Length, currentIndex + 1);
        SwitchPage(target);
    }

    public void PreviousPage()
    {
        int target = Mathf.Max(0, currentIndex - 1);
        SwitchPage(target);
    }
}