using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slide up and dow.
/// Canvas mode: Expand
/// listPanel(slider): Stretch both, Pivot top
/// startPoint: Stretch both, Pivot bottom 
/// </summary>
public class UIFishListPanel : MonoBehaviour
{
    // fish counter
    [SerializeField] private Text currentFishCountText;
    [SerializeField] private Text maxFishCountText;

    // list panel mask
    [SerializeField] private Image screenMask;
    [SerializeField] private float maskAlpha = 0.7f;

    [SerializeField] private Transform canvasForlockIcon;

    // mask for lock icons on fish items.
    [SerializeField] private Transform lockIconMask;

    // the panel to slide.
    // anchor: stretch both
    // pivot top (0.5, 1)
    [SerializeField] private RectTransform listPanel;

    // locate the start point of sliding.
    // anchor: stretch both, the same as listPanel
    // pivot bottom (0.5, 0), to align with canvas's bottom
    [SerializeField] private RectTransform startPoint;

    // different aquarium and list
    [SerializeField] private GameObject hatcheryContainer;
    [SerializeField] private GameObject metaverseContainer;

    // for calculating mask rect
    [SerializeField] private RectTransform activeScrollViewPort;
    [SerializeField] private RectTransform archiveScrollViewPort;
    [SerializeField] private RectTransform metaverseScrollViewPort;

    // content node in ScrollView
    [SerializeField] private GridLayoutGroup activeGridLayoutGroup;
    [SerializeField] private GridLayoutGroup archiveGridLayoutGroup;
    [SerializeField] private GridLayoutGroup metaverseGridLayoutGroup;

    // slide button icon
    [SerializeField] private GameObject arrowUp;
    [SerializeField] private GameObject arrowDown;
    [SerializeField] private Text slideButtonText;

    // tabs
    [SerializeField] private GameObject activeTabOnImage;
    [SerializeField] private GameObject activeTabOffImage;
    [SerializeField] private GameObject archiveTabOnImage;
    [SerializeField] private GameObject archiveTabOffImage;

    // tab contents
    [SerializeField] private GameObject activeTabContent;
    [SerializeField] private GameObject archiveTabContent;

    // canvas camera
    [SerializeField] private Camera canvasCamera;

    // model camera
    [SerializeField] private Camera modelCamera;


    // cell parameters
    [SerializeField] private int cellSpacing = 10;
    [SerializeField] private int iPadColNumber = 3;
    [SerializeField] private int iPhoneColNumber = 4;

    //
    [SerializeField] private UIFishListItem fishListItemPrefab;
    [SerializeField] private float slideDuration = 0.5f;

    private Vector2 startPosition; // slide start
    private Vector2 endPosition; // slide end
    private int cellColNumber;
    private bool panelSlidedOut;
    private bool isActiveOn = true; // the active tab is selected
    private List<GameObject> activeItems = new(); // fishes
    private List<GameObject> archiveItems = new(); // fishes
    private List<GameObject> metaverseItems = new(); // fishes


    #region Unity

    private IEnumerator Start()
    {
        screenMask.gameObject.SetActive(false);

        // Animation
        FixedlyChangePivot(startPoint, new Vector2(0.5f, 1)); // match the pivot of start point to slider
        startPosition = startPoint.anchoredPosition;
        endPosition = listPanel.anchoredPosition;
        listPanel.anchoredPosition = startPosition;

        arrowUp.SetActive(true);
        arrowDown.SetActive(false);
        slideButtonText.SetLocalization(GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId
            ? "txt_hatchery"
            : "txt_metaverse");

        // Main list
        hatcheryContainer.SetActive(false); // hide when invisible
        metaverseContainer.SetActive(false);

        // Hatchery tabs
        activeTabContent.SetActive(true);
        archiveTabContent.SetActive(false);

        yield return new WaitForEndOfFrame();

        // Set GridLayoutGroup parameters
        cellColNumber = 1.0f * Screen.width / Screen.height < 1.5 ? iPadColNumber : iPhoneColNumber;
        float cellWidth = activeGridLayoutGroup.GetComponent<RectTransform>().rect.width - cellSpacing * (cellColNumber + 1);
        cellWidth /= cellColNumber;
        float cellHeight = activeGridLayoutGroup.cellSize.y / activeGridLayoutGroup.cellSize.x * cellWidth;
        activeGridLayoutGroup.padding = new RectOffset(cellSpacing, cellSpacing, cellSpacing, cellSpacing);
        activeGridLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
        activeGridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
        archiveGridLayoutGroup.padding = activeGridLayoutGroup.padding;
        archiveGridLayoutGroup.spacing = activeGridLayoutGroup.spacing;
        archiveGridLayoutGroup.cellSize = activeGridLayoutGroup.cellSize;
        metaverseGridLayoutGroup.padding = activeGridLayoutGroup.padding;
        metaverseGridLayoutGroup.spacing = activeGridLayoutGroup.spacing;
        metaverseGridLayoutGroup.cellSize = activeGridLayoutGroup.cellSize;

        // setup lock icon mask
        if (GameSave.SelectedAquarium == GameSetting.MetaverseAquariumId)
        {
            lockIconMask.gameObject.SetActive(true);
            lockIconMask.parent = canvasForlockIcon;
        }

        // disable model camera in scene to avoid affecting main camera
        modelCamera.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        UpdateAllFishItems();
        ImageDataManager.Instance.ImageDataUpdatedEvent += OnImageDataUpdated; // in case of removing fish.
        ImageDataManager.Instance.InAquariumImageDataUpdatedEvent += UpdateAllFishItemsInAquariumStates;
    }

    private void OnDisable()
    {
        ImageDataManager.Instance.ImageDataUpdatedEvent -= OnImageDataUpdated;
        ImageDataManager.Instance.InAquariumImageDataUpdatedEvent -= UpdateAllFishItemsInAquariumStates;
    }

    private void Update()
    {
        if (GameSave.SelectedAquarium == GameSetting.MetaverseAquariumId)
        {
            lockIconMask.position = CurrentViewPort.position;
        }

        UpdateFishCount();
        modelCamera.GetComponent<CameraScissor>().scissorRect = GetItemContainerScreenRect();
    }

    #endregion

    #region Panel Animation

    public void SwitchPanel()
    {
        if (panelSlidedOut)
        {
            MoveToStart();
            arrowUp.SetActive(true);
            arrowDown.SetActive(false);
        }
        else
        {
            MoveToEnd();
            arrowUp.SetActive(false);
            arrowDown.SetActive(true);
        }
    }

    private void MoveToStart()
    {
        listPanel.DOAnchorPos(startPosition, slideDuration).OnComplete(() =>
        {
            screenMask.gameObject.SetActive(false);
            hatcheryContainer.SetActive(false);
            metaverseContainer.SetActive(false);
            GetItemContainerScreenRect();
        });
        screenMask.DOFade(0, slideDuration);
        panelSlidedOut = false;
    }

    private void MoveToEnd()
    {
        hatcheryContainer.SetActive(GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId);
        metaverseContainer.SetActive(GameSave.SelectedAquarium == GameSetting.MetaverseAquariumId);

        listPanel.DOAnchorPos(endPosition, slideDuration).OnComplete(() =>
        {
            GetItemContainerScreenRect();
        });
        screenMask.gameObject.SetActive(true);
        screenMask.DOFade(maskAlpha, slideDuration);
        panelSlidedOut = true;
    }

    private void FixedlyChangePivot(RectTransform rectTransform, Vector2 pivot)
    {
        Vector3 localPos = rectTransform.localPosition;
        rectTransform.pivot = pivot;
        rectTransform.localPosition = localPos;
    }

    #endregion

    #region Preview Camera

    /// Rect for 3D model camera (0 ~ 1)
    private Rect GetItemContainerScreenRect()
    {
        var worldCorners = new Vector3[4];
        var screenCorners = new Vector3[4];
        CurrentViewPort.GetWorldCorners(worldCorners);
        for (var i = 0; i < worldCorners.Length; i++)
        {
            screenCorners[i] = canvasCamera.WorldToScreenPoint(worldCorners[i]);
        }

        var positiveScreenCornerX = Mathf.Max(0f, 1.0f * screenCorners[0].x);
        var positiveScreenCornerY = Mathf.Max(0f, 1.0f * screenCorners[0].y);
        var positiveWidth = Mathf.Max(0.001f, 1.0f * (screenCorners[2].x - positiveScreenCornerX) / Screen.width);
        var positiveHeight = Mathf.Max(0.001f, 1.0f * (screenCorners[2].y - positiveScreenCornerY) / Screen.height);

        var result = new Rect(positiveScreenCornerX / Screen.width,
            positiveScreenCornerY / Screen.height, positiveWidth, positiveHeight);
        return result;
    }

    private RectTransform CurrentViewPort => GameSave.SelectedAquarium != GameSetting.MetaverseAquariumId
        ? isActiveOn
            ? activeScrollViewPort
            : archiveScrollViewPort
        : metaverseScrollViewPort;

    #endregion

    #region Hatchery Tabs

    public void OnActiveTabToggleChanged(bool value)
    {
        isActiveOn = value;
        activeTabOnImage.SetActive(value);
        activeTabOffImage.SetActive(!value);
        activeTabContent.SetActive(value);
    }

    public void OnArchiveTabToggleChanged(bool value)
    {
        isActiveOn = value;
        archiveTabOnImage.SetActive(value);
        archiveTabOffImage.SetActive(!value);
        archiveTabContent.SetActive(value);
    }

    #endregion

    #region Fish Counter

    private void UpdateFishCount()
    {
        currentFishCountText.text = (GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId
            ? ImageDataManager.Instance.InHatcheryImageDataList.Count
            : ImageDataManager.Instance.InMetaverseImageDataList.Count).ToString();
        maxFishCountText.text = GameSetting.MaxFishNumberInAquarium.ToString();
    }

    #endregion

    #region Main List

    private void UpdateAllFishItems()
    {
        if (GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId)
        {
            UpdateHatcheryFishItems(ref activeItems, ImageDataManager.Instance.ActiveImageDataList, activeGridLayoutGroup.transform);
            UpdateHatcheryFishItems(ref archiveItems, ImageDataManager.Instance.ArchivedImageDataList, archiveGridLayoutGroup.transform);
        }
        else
        {
            UpdateMetaverseFishItems(ref metaverseItems, ImageDataManager.Instance.NftImageDataList, metaverseGridLayoutGroup.transform);
        }
    }

    private void UpdateHatcheryFishItems(ref List<GameObject> items, in List<ImageData> imageDataList, Transform container)
    {
        foreach (var item in items) Destroy(item);
        items.Clear();

        for (var i = 0; i < imageDataList.Count; i++)
        {
            var item = CreateFishItem(ref items, container);
            item.InitAsHatcheryArtwork(imageDataList[i]);
        }
    }

    private void UpdateMetaverseFishItems(ref List<GameObject> items, in List<ImageData> imageDataList, Transform container)
    {
        int currentPlanNumber; // the capacity of current IAP plan
        int nextPlanNumber; // the capacity of the next IAP plan
        int currentPackage = DataCenter.Instance.GetBandPurchaseStatus();
        switch (currentPackage)
        {
            case 0:
                currentPlanNumber = 0;
                nextPlanNumber = 5;
                break;
            case 1:
                currentPlanNumber = 5;
                nextPlanNumber = 10;
                break;
            case 2:
                currentPlanNumber = 10;
                nextPlanNumber = 20;
                break;
            default:
                currentPlanNumber = 20;
                nextPlanNumber = 20;
                break;
        }

        foreach (var item in items) Destroy(item);
        items.Clear();

        // saved unlocker number
        int originalUnlockedNumber = imageDataList.Count(t => t.Unlocked);

        // if the unlocked fishes are less than targetUnlockedNumber
        int unusedUnlockedNumber = currentPlanNumber - originalUnlockedNumber;

        int unlockedNumber = 0;

        // add unlocked fish items
        for (var i = 0; i < imageDataList.Count; i++)
        {
            if (unlockedNumber >= currentPlanNumber)
            {
                imageDataList[i].Unlocked = false; // the number of unlocked fishes is greater than the plan
                continue;
            }


            // already unlocked
            if (imageDataList[i].Unlocked)
            {
                var item = CreateFishItem(ref items, container);
                item.InitAsMetaverseArtwork(imageDataList[i], lockIconMask);
                unlockedNumber++;
            }
            // change locked fish to unlocked
            else if (unusedUnlockedNumber > 0)
            {
                imageDataList[i].Unlocked = true;
                var item = CreateFishItem(ref items, container);
                item.InitAsMetaverseArtwork(imageDataList[i], lockIconMask);
                unlockedNumber++;
                unusedUnlockedNumber--;
            }
        }

        // add locked fishes
        for (var i = 0; i < imageDataList.Count; i++)
        {
            if (!imageDataList[i].Unlocked)
            {
                var item = CreateFishItem(ref items, container);
                item.InitAsMetaverseArtwork(imageDataList[i], lockIconMask);
            }
        }

        // add placeholder

        // include unlocked placeholder
        for (int i = imageDataList.Count; i < nextPlanNumber; i++)
        {
            var item = CreateFishItem(ref items, container);
            item.InitAsPlaceHolder(lockIconMask, i >= currentPlanNumber);
        }

        // only locked placeholder
        // if (currentPlanNumber != nextPlanNumber)
        // {
        //     for (int i = imageDataList.Count; i < nextPlanNumber; i++)
        //     {
        //         var item = CreateFishItem(ref items, container);
        //         item.InitAsPlaceHolder(lockIconMask, true);
        //     }
        // }

        // insert default fishes in front of the list.
        foreach (ImageData data in ImageDataManager.Instance.DefaultImageDataList)
        {
            var item = CreateFishItem(ref items, container, true);
            item.InitAsMetaverseArtwork(data, lockIconMask);
        }

        ImageDataManager.Instance.FillImageDataLists(false); // rebuild in-aquarium list for FishTank.Start to create fishes
        ImageDataManager.Instance.SaveDataToDisk();
    }

    private UIFishListItem CreateFishItem(ref List<GameObject> items, Transform container, bool addToHead = false)
    {
        UIFishListItem item = Instantiate(fishListItemPrefab, container);
        if (addToHead)
        {
            item.transform.SetSiblingIndex(0);
            items.Insert(0, item.gameObject);
        }
        else
        {
            items.Add(item.gameObject);
        }

        item.AquariumId = GameSave.SelectedAquarium;
        item.name += items.Count.ToString();
        return item;
    }

    private void OnImageDataUpdated()
    {
        UpdateAllFishItems();
    }

    private void UpdateAllFishItemsInAquariumStates()
    {
        if (GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId)
        {
            foreach (GameObject item in activeItems)
            {
                UIFishListItem fishListItem = item.GetComponent<UIFishListItem>();
                fishListItem.UpdateInAquariumState();
            }

            foreach (GameObject item in archiveItems)
            {
                UIFishListItem fishListItem = item.GetComponent<UIFishListItem>();
                fishListItem.UpdateInAquariumState();
            }
        }
        else
        {
            foreach (GameObject item in metaverseItems)
            {
                UIFishListItem fishListItem = item.GetComponent<UIFishListItem>();
                fishListItem.UpdateInAquariumState();
            }
        }
    }

    #endregion
}