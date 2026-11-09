using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class TabSwitchEvent : UnityEvent<int> { }

public class TabsHolder : MonoBehaviour
{
    public TabSwitchEvent OnTagSwicth;
    public int DefaultTab = 0;
    public bool SetTabOnStart = false;
    public Button[] TabsButton;

    protected int currentTab = -1;
    protected Sprite cachedNormalSprite = null;

    void Start()
    {
        if (SetTabOnStart)
        {
            SwitchPage(DefaultTab);
        }

        for (int i = 0; i < TabsButton.Length; i++)
        {
            int tapPage = i;
            TabsButton[i].onClick.AddListener(() =>
            {
                SwitchPage(tapPage);
            });
        }
    }

    public void SwitchPage(int targetTab)
    {
        if (targetTab != currentTab)
        {
            if (cachedNormalSprite != null && currentTab != -1)
            {
                TabsButton[currentTab].image.sprite = cachedNormalSprite;
            }

            if (TabsButton[targetTab].spriteState.pressedSprite != null)
            {
                cachedNormalSprite = TabsButton[targetTab].image.sprite;
                TabsButton[targetTab].image.sprite = TabsButton[targetTab].spriteState.pressedSprite;
            }

            currentTab = targetTab;

            if (OnTagSwicth != null)
            {
                OnTagSwicth.Invoke(targetTab);
            }
        }
    }
}