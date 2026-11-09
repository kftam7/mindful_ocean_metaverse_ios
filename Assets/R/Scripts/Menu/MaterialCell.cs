using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialCell : MonoBehaviour
{
    public Image materialImage;
    public GameObject locker;
    public Text materialName;

    protected PrintingMaterialData data;

    public PrintingMaterialData MaterialData
    {
        get { return data; }
    }

    public void SetData(PrintingMaterialData data, bool isLock)
    {
        this.data = data;
        this.locker.SetActive(isLock);
        this.materialName.SetLocalization(data.ColoringPaperName);
        materialImage.sprite = SpriteDicCenter.Instance.PrintingMaterialSpriteDic.GetSpriteByName(data.Perview);
        if (isLock)
        {
            this.GetComponent<Toggle>().interactable = false;
        }
    }

    public void Select(bool isON)
    {
        PrintingUIManager.Instance.SetSelectedImage(data, true);
    }
}