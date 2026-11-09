using System.Collections;
using System.Collections.Generic;
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class UIReplaceFishListPopup : SimplePopup
{
    [SerializeField] private ScrollRect scrollRect;

    // for calculating mask rect
    [SerializeField] private RectTransform scrollViewPort;

    // content node in ScrollView
    [SerializeField] private HorizontalLayoutGroup layoutGroup;

    // canvas camera
    private Camera canvasCamera;

    // model camera
    private Camera modelCamera;

    // cell parameters
    [SerializeField] private int cellSpacing = 10;

    //
    [SerializeField] private UIReplaceFishListItem fishListItemPrefab;

    private float cellHeight;
    private List<GameObject> items = new(); // fishes
    private bool isStarted;

    [HideInInspector]
    public ModelPreviewPopup ModelPreviewPopup;


    #region Unity

    private IEnumerator Start()
    {
        canvasCamera = GameObject.FindWithTag("TopPopupCamera").GetComponent<Camera>();
        modelCamera = GameObject.FindWithTag("ReplaceFishListModelCamera").GetComponent<Camera>();
        yield return new WaitForEndOfFrame();

        cellHeight = layoutGroup.GetComponent<RectTransform>().rect.height;
        layoutGroup.padding = new RectOffset(cellSpacing, cellSpacing, cellSpacing, cellSpacing);
        layoutGroup.spacing = cellSpacing;

        // disable model camera in scene to avoid affecting main camera
        modelCamera.gameObject.SetActive(true);
        UpdateFishItems();
        isStarted = true;
    }

    protected override void InitPopup()
    {
        base.InitPopup();
        if (isStarted) UpdateFishItems();
    }

    public override void Close()
    {
        NativeHelper.GenerateImpactFeedback();
        base.Close();
        foreach (var item in items) Destroy(item);
        items.Clear();
        scrollRect.horizontalNormalizedPosition = 0;
    }

    private void Update()
    {
        modelCamera.GetComponent<CameraScissor>().scissorRect = GetItemContainerScreenRect();
    }

    #endregion

    /// Rect for 3D model camera (0 ~ 1)
    private Rect GetItemContainerScreenRect()
    {
        var worldCorners = new Vector3[4];
        var screenCorners = new Vector3[4];
        scrollViewPort.GetWorldCorners(worldCorners);
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

    #region Main List

    private void UpdateFishItems()
    {
        List<ImageData> imageDataList = GetLockedFishes();
        foreach (var item in items) Destroy(item);
        items.Clear();

        for (var i = 0; i < imageDataList.Count; i++)
        {
            var item = CreateFishItem(ref items, layoutGroup.transform);
            item.Init(imageDataList[i]);
        }
    }

    private List<ImageData> GetLockedFishes()
    {
        List<ImageData> result = new List<ImageData>();
        List<ImageData> imageDataList = ImageDataManager.Instance.NftImageDataList;
        for (var i = 0; i < imageDataList.Count; i++)
        {
            if (!imageDataList[i].Unlocked || imageDataList[i] == DataCenter.Instance.CurrentArtwork)
            {
                result.Add(imageDataList[i]);
            }
        }

        return result;
    }

    private UIReplaceFishListItem CreateFishItem(ref List<GameObject> items, Transform container)
    {
        UIReplaceFishListItem item = Instantiate(fishListItemPrefab);
        items.Add(item.gameObject);
        item.name += items.Count.ToString();
        item.ModelPreviewPopup = ModelPreviewPopup;
        var itemRectTransform = item.GetComponent<RectTransform>();
        var originalWidth = itemRectTransform.rect.width;
        var originalHeight = itemRectTransform.rect.height;
        itemRectTransform.sizeDelta = new Vector2(originalWidth / originalHeight * cellHeight, cellHeight);
        itemRectTransform.SetParent(container, false);
        return item;
    }

    #endregion
}