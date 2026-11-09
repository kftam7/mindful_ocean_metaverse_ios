using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

public class FooterSpecialHandle : MonoBehaviour
{
    public GameObject SpecialButtonObject;
    public GameObject NormalButtonObject;

    public GameObject SpecialButtonAlert;
    public GameObject NormalButtonAlert;

    public Button[] ButtonList;

    public GameObject[] ButtonAlert;

    public string[] TabModelName = new string[] { "Dolphin", "Shark", "Turtle", "Tuna" };
    public GameObject[] TabAlert;

    public Transform PageContainer;

    private const int SpecialPageIndex = 5;
    private const int SpecialButtonIndex = 2;

    private readonly string[] normalTextKey = new string[] { "txt_ebook_introduction", "txt_conservation_status", "txt_distribution", "txt_threats", "txt_conservation" };
    private readonly string[] specialTextKey = new string[] { "txt_plastic_pollution", "txt_plastic_estimate", "txt_plastic_plan", "txt_plastic_influence", "txt_plastic_ways" };

    public SelectionButtonGroup FooterButtonGroup;

    void Start()
    {
        RefreshTabAlert();
    }

    public void RefreshTabAlert()
    {
        for (int i = 0; i < TabAlert.Length && i < TabModelName.Length; i++)
        {
            TabAlert[i].SetActive(DataCenter.Instance.GetIsNewTab(TabModelName[i]));
        }
    }

    public void RefreshButtonAlert(string model)
    {
        for (int i = 0; i < ButtonAlert.Length; i++)
        {
            ButtonAlert[i].SetActive(DataCenter.Instance.GetIsNewPage(model, i));
        }
    }

    public void RefreshButtonText(int page)
    {
        for (int i = 0; i < ButtonList.Length; i++)
        {
            Localize l = ButtonList[i].transform.parent.GetComponentInChildren<Localize>();
            if (l != null)
            {
                l.Set(page == 5 ? specialTextKey[i] : normalTextKey[i]);
            }
        }
    }

    public void TurnToPage(int page)
    {
        bool specialPage = SpecialPageIndex == page;

        //Handle special change
        SpecialButtonObject.SetActive(specialPage);
        NormalButtonObject.SetActive(!specialPage);

        SpecialButtonAlert.SetActive(specialPage);
        NormalButtonAlert.SetActive(!specialPage);

        //Set Special button to normal set
        ButtonList[SpecialButtonIndex] = specialPage ? SpecialButtonObject.GetComponentInChildren<Button>() : NormalButtonObject.GetComponentInChildren<Button>();
        ButtonAlert[SpecialButtonIndex] = specialPage ? SpecialButtonAlert : NormalButtonAlert;

        //Clear action and add new action to button
        if (page == 0)
        {
            ShowPage("Prefabs/Ebook/web/web");
        }
        else
        {
            EbookData data;
            if (DataCenter.Instance.EbookDataDic.TryGetValue(page.ToString(), out data))
            {
                int unlockedIndex = DataCenter.Instance.GetEbookUnlockedIndex(data.Model);

                for (int i = 0; i < ButtonList.Length; i++)
                {
                    bool isOpen = i <= unlockedIndex;

                    ButtonList[i].onClick.RemoveAllListeners();
                    ButtonList[i].interactable = isOpen;

                    string prefab = data.Path + data.Pages[i];
                    string modelName = data.Model;
                    int openIndex = i;

                    ButtonList[i].onClick.AddListener(() =>
                    {
                        ShowPage(prefab);
                        if (DataCenter.Instance.SetOpenedPage(modelName, openIndex))
                        {
                            RefreshTabAlert();
                            RefreshButtonAlert(modelName);
                        }
                    });

                    if (i == 0)
                    {
                        GameObject p = ShowPage(prefab);
                        if (p != null)
                        {
                            HiddenUnlockPage hidden = p.GetComponentInChildren<HiddenUnlockPage>();
                            if (hidden != null) hidden.SetStatus(!isOpen);
                        }
                    }
                }

                FooterButtonGroup.ResetListener();
                if (unlockedIndex >= 0)
                {
                    ButtonList[0].onClick.Invoke();
                }

                RefreshButtonAlert(data.Model);
                RefreshButtonText(page);
            }
        }
    }

    protected GameObject currentPage = null;
    protected string pageName = "";

    public GameObject ShowPage(string pagePrefab)
    {
        if (pageName != pagePrefab)
        {
            //New page
            Destroy(currentPage);
            currentPage = GameObject.Instantiate(Resources.Load(pagePrefab) as GameObject, PageContainer);
            HiddenUnlockPage hidden = currentPage.GetComponentInChildren<HiddenUnlockPage>();
            if (hidden != null)
            {
                hidden.SetStatus(false);
            }

            return currentPage;
        }

        return null;
    }
}