using System.Collections;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class UIFishListItem : MonoBehaviour
{
    [SerializeField] private Button previewButton;
    [SerializeField] private GameObject fishPlaceHolder;
    [SerializeField] private Transform lockIconAnchor;
    [SerializeField] private Transform lockIcon;

    [SerializeField] private Toggle homeToggle;
    [SerializeField] private Transform modelAnchor;
    private ImageData imageData;
    private GameObject fishModel;
    private CharacterData characterData;
    private Transform lockIconParent;
    private bool isPlaceHolder;
    private bool isLocked;
    public string AquariumId { get; set; }

    private void OnEnable()
    {
        SetAnimPose();
    }

    private void LateUpdate()
    {
        if (lockIcon.gameObject.activeSelf)
        {
            lockIcon.position = lockIconAnchor.position;
        }
    }

    private void OnDestroy()
    {
        ShowLockIcon(false);
        Destroy(fishModel);
    }

    public void InitAsPlaceHolder(Transform lockIconParent, bool locked = true)
    {
        isPlaceHolder = true;
        isLocked = locked;
        homeToggle.gameObject.SetActive(false);
        this.lockIconParent = lockIconParent;
        fishPlaceHolder.SetActive(true);
        ShowLockIcon(locked);
        previewButton.interactable = true;
        previewButton.onClick.AddListener(ShowFishHintPopup);
    }

    public void InitAsHatcheryArtwork(ImageData data)
    {
        isPlaceHolder = false;
        isLocked = false;
        imageData = data;
        homeToggle.isOn = imageData.InAquarium;
        fishPlaceHolder.SetActive(false);
        ShowLockIcon(false);
        CreatePreviewModel();
        previewButton.interactable = true;
        previewButton.onClick.AddListener(ShowModelPreview);
    }

    public void InitAsMetaverseArtwork(ImageData data, Transform lockIconParent)
    {
        isPlaceHolder = false;
        isLocked = !data.Unlocked;
        this.lockIconParent = lockIconParent;
        imageData = data;
        homeToggle.isOn = imageData.InAquarium;
        fishPlaceHolder.SetActive(false);
        ShowLockIcon(!data.Unlocked);
        previewButton.interactable = true;
        homeToggle.gameObject.SetActive(data.Unlocked);
        CreatePreviewModel();
        if (data.Unlocked)
        {
            previewButton.onClick.AddListener(ShowModelPreview);
        }
        else
        {
            previewButton.onClick.AddListener(ShowFishHintPopup);
        }
    }

    public void UpdateInAquariumState()
    {
        if (imageData != null)
        {
            homeToggle.isOn = imageData.InAquarium;
        }
    }

    public void OnHomeToggleChanged(bool value)
    {
        var fishTank = FindObjectOfType<FishTank>();
        if (fishTank == null) return;
        if (value)
        {
            ImageDataManager.Instance.AddImageDataIntoAquarium(imageData, true, true);
            FishEntity fishEntity = fishTank.FindFish(imageData);
            if (fishEntity == null)
            {
                fishTank.CreateFish(imageData, 0, Vector3.zero, 0);
            }
        }
        else
        {
            ImageDataManager.Instance.RemoveImageDataFromAquarium(imageData, true, true);
            FishEntity fishEntity = fishTank.FindFish(imageData);
            if (fishEntity != null)
            {
                fishTank.RemoveFish(fishEntity);
            }
        }
    }

    public void ShowModelPreview()
    {
        NativeHelper.GenerateImpactFeedback();
        DataCenter.Instance.CurrentArtwork = imageData;
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ModelPreviewPopup);
    }

    public void ShowFishHintPopup()
    {
        PopupManager.Instance.QuickShowPopup(PopupPrefab.FishHintPopup, true);
        var popup = PopupManager.Instance.ActivePopup as FishHintPopup;
        if (isPlaceHolder)
        {
            if (isLocked)
            {
                popup.ShowText("txt_locked_blank_fish_hint");
                popup.SetPurchaseButton(true);
            }
            else
            {
                popup.ShowTitle("txt_blank_fish_hint");
                popup.SetPurchaseButton(false);
            }
        }
        else
        {
            if (isLocked)
            {
                if (DataCenter.Instance.GetBandPurchaseStatus() == 3
                    && DataCenter.Instance.IsAccessoryPurchased())
                {
                    popup.ShowText("txt_locked_fish_all_purchased_hint");
                    popup.SetPurchaseButton(false);
                }
                else
                {
                    popup.ShowText("txt_locked_fish_hint");
                    popup.SetPurchaseButton(true);
                }
            }
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
            this.characterData = characterData;
            fishModel = ReuseableObjectManager.Instantiate(FishTank.ModelPath + characterData.FishTankModelName, Vector3.zero, Quaternion.identity);
            fishModel.AddComponent<RectTransform>();
            fishModel.transform.SetParent(modelAnchor);
            fishModel.transform.localPosition = new Vector3(0, characterData.ListYOffset, 0);
            fishModel.transform.localEulerAngles = new Vector3(0, 80, 0);
            fishModel.transform.localScale = Vector3.one * characterData.ListSize;

            SetSkin(fishModel, imageData);
            if (gameObject.activeInHierarchy) SetAnimPose();
            ChangeLayersRecursively(fishModel.transform, 24);
        }
    }

    private void SetAnimPose()
    {
        if (fishModel != null)
        {
            fishModel.GetComponent<Animator>().enabled = true;
            fishModel.GetComponent<Animator>().Play("swim", 0, characterData.ListAnimPosition);
            CoroutineManager.Instance.StartCoroutine(DelayDisableAnimator());
        }
    }

    private IEnumerator DelayDisableAnimator()
    {
        yield return new WaitForFixedUpdate();
        fishModel.GetComponent<Animator>().enabled = false;
    }

    private void ShowLockIcon(bool value)
    {
        if (value)
        {
            lockIcon.gameObject.SetActive(true);
            lockIcon.parent = lockIconParent;
        }
        else
        {
            lockIcon.gameObject.SetActive(false);
            lockIcon.parent = lockIconAnchor;
        }
    }

    private void SetSkin(GameObject fish, ImageData imageData)
    {
        if (!string.IsNullOrEmpty(imageData.Url))
        {
            SetSkinFromUrl(fish, imageData.Url);
        }
        else
        {
            SetSkinFromImageFile(fish, imageData.ImageFile);
        }
    }

    private void SetSkinFromImageFile(GameObject fish, string fullFileName)
    {
        Texture2D skin = TextureCache.Instance.Load(fullFileName, true);
        fish.GetComponentInChildren<Renderer>(true).material.SetTexture("_MainTex", skin);
    }

    private void SetSkinFromUrl(GameObject fish, string imageUrl)
    {
        ServerCenter.Instance.GetImageFromCacheOrUrl(imageUrl, true, texture =>
        {
            fish.GetComponentInChildren<Renderer>(true).material.SetTexture("_MainTex", texture);
        });
    }
}