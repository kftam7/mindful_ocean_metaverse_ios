using System.Collections;
using ProGamer;
using UnityEngine;

public class UIReplaceFishListItem : MonoBehaviour
{
    [SerializeField] private Transform modelAnchor;
    private ImageData imageData;
    private GameObject fishModel;

    [HideInInspector]
    public ModelPreviewPopup ModelPreviewPopup;

    private void OnDestroy()
    {
        Destroy(fishModel);
    }

    public void Init(ImageData data)
    {
        imageData = data;
        CreatePreviewModel();
    }

    public void Replace()
    {
        var current = DataCenter.Instance.CurrentArtwork;
        if (imageData != current)
        {
            current.Unlocked = false;
            imageData.Unlocked = true;

            if (current.InAquarium)
            {
                var fishTank = FindObjectOfType<FishTank>();
                if (fishTank != null)
                {
                    ImageDataManager.Instance.RemoveImageDataFromAquarium(current, false, false);
                    ImageDataManager.Instance.AddImageDataIntoAquarium(imageData, true, true);
                    FishEntity fishEntity = fishTank.FindFish(imageData);
                    if (fishEntity == null)
                    {
                        fishTank.CreateFish(imageData, 0, Vector3.zero, 0);
                    }

                    fishEntity = fishTank.FindFish(current);
                    if (fishEntity != null)
                    {
                        fishTank.RemoveFish(fishEntity);
                    }
                }
            }

            DataCenter.Instance.CurrentArtwork = imageData;
            ModelPreviewPopup.UpdateModel();
        }
    }

    private void ChangeLayersRecursively(Transform xform, int layer)
    {
        xform.gameObject.layer = layer;
        for (int i = 0; i < xform.childCount; i++)
        {
            xform.GetChild(i).gameObject.layer = layer;
            ChangeLayersRecursively(xform.GetChild(i), layer);
        }
    }

    private void CreatePreviewModel()
    {
        if (DataCenter.Instance.CharacterDataDic.TryGetValue(imageData.GetModelName(), out var characterData))
        {
            fishModel = ReuseableObjectManager.Instantiate(FishTank.ModelPath + characterData.FishTankModelName, Vector3.zero, Quaternion.identity);
            fishModel.AddComponent<RectTransform>();
            fishModel.transform.SetParent(modelAnchor);
            fishModel.transform.localPosition = new Vector3(0, characterData.ListYOffset, 0);
            fishModel.transform.localEulerAngles = new Vector3(0, 80, 0);
            fishModel.transform.localScale = Vector3.one * characterData.ListSize * 0.8f;

            Texture2D skin = TextureCache.Instance.Load(imageData.ImageFile, true);
            fishModel.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
            fishModel.GetComponent<Animator>().enabled = true;
            fishModel.GetComponent<Animator>().Play("swim", 0, characterData.ListAnimPosition);
            CoroutineManager.Instance.StartCoroutine(DelayDisableAnimator());
            ChangeLayersRecursively(fishModel.transform, 28);
        }
    }

    private IEnumerator DelayDisableAnimator()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        fishModel.GetComponent<Animator>().enabled = false;
    }
}