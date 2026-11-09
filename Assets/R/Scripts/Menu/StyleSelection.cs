using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StyleSelection : SceneInstance<StyleSelection>
{
    public ScrollRect Scrollview;
    public RectTransform ParentCanvas;
    public Animator NewBookAnimator;

    void Start()
    {
        InitScrollview();
    }

    protected List<StyleSelectButton> mStyleButtonList = new List<StyleSelectButton>();

    void InitScrollview()
    {
        float canvasWidth = ParentCanvas.sizeDelta.x;
        float cellWidth = 0;

        foreach (CollectionData c in DataCenter.Instance.CollectionDataDic.Values)
        {
            GameObject go = Instantiate(Resources.Load("Prefabs/Menu/StyleButton") as GameObject, Scrollview.content);

            go.transform.localScale = Vector3.one;
            StyleSelectButton b = go.GetComponent<StyleSelectButton>();
            b.SetButton(c);
            SizeTransitByPosition s = go.GetComponent<SizeTransitByPosition>();
            s.ParentCanvas = ParentCanvas;

            if (cellWidth == 0) cellWidth = (go.transform as RectTransform).sizeDelta.x;
            mStyleButtonList.Add(b);
        }

        HorizontalLayoutGroup layout = Scrollview.content.GetComponent<HorizontalLayoutGroup>();
        int p = Mathf.RoundToInt(canvasWidth / 2 - cellWidth / 2);
        layout.padding.left = p;
        layout.padding.right = p;

        StartCoroutine(SnapCell());
    }

    IEnumerator SnapCell()
    {
        yield return new WaitForEndOfFrame();
        (Scrollview as SnapScrollView).SnapToIndex(0);
    }

    public void OpenNewBook(CollectionData data)
    {
        SetStyle(data);

        if (!NewBookAnimator.gameObject.activeSelf)
        {
            NewBookAnimator.gameObject.SetActive(true);
        }

        NewBookAnimator.GetComponentInChildren<BookDetail>().SetDetail(data);
        NewBookAnimator.CrossFade("flippage", 0);

        ProGamer.SoundManager.Instance.Play("ocean_saver_create_new_aquarium_sfx");
    }

    public void EnterAquarium()
    {
        SceneTransitHandler.LoadScene("Aquarium");
    }

    public void OnStyleSnapped(int i)
    {
        if (mStyleButtonList.Count > i)
        {
            mStyleButtonList[i].SetButtonActive(true);
            ProGamer.SoundManager.Instance.Play("ocean_saver_next_aquarium_sfx");
        }
    }

    public void SetStyle(CollectionData data)
    {
        // string id = ImageDataWriter.GetGUID();
        //
        // int index = GameSave.AquariumIndex;
        // GameSave.AquariumIndex = index + 1;
        //
        // DataCenter.Instance.AddAquarium(new AquariumData(id, data.Theme, data, index));
        // GameSave.SelectedAquarium = id;
        // ImageDataManager.Instance.SetImageDataSaveKey(id);
    }

    public void StartSelect()
    {
        foreach (StyleSelectButton b in mStyleButtonList)
        {
            b.SetButtonActive(false);
        }
    }
}