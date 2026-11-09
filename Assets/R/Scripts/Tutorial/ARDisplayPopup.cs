using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public class ARDisplayPopup : GeneralTutorialProgress
{
    public Transform Container;

    public override void StartProgress(TutorialStep step)
    {
        base.StartProgress(step);

        string id = GameSave.LastSavedImageId;
        ImageData data = null;

        foreach (var idata in ImageDataManager.Instance.ImageDataList)
        {
            if (id == idata.Id)
            {
                data = idata;
                break;
            }
        }

        CharacterData characterData;

        if (data != null && DataCenter.Instance.CharacterDataDic.TryGetValue(data.GetModelName(), out characterData))
        {
            Debug.Log("Load: " + FishTank.ModelPath + characterData.PrefabPath);

            GameObject go = ReuseableObjectManager.Instantiate(FishTank.ModelPath + characterData.FishTankModelName, Vector3.zero, Quaternion.identity);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(Container);
            go.transform.localPosition = new Vector3(0, -40, 0);
            go.transform.localEulerAngles = new Vector3(0, -100, 0);
            go.transform.localScale = Vector3.one * 400;

            Texture2D skin = TextureCache.Instance.Load(data.ImageFile, true);
            go.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
        }
    }
}