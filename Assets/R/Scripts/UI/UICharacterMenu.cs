using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterMenu : MonoBehaviour
{
    [SerializeField] private UICharacterMenuScrollView _scrollView;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private UIMainMenuPriceDisplay _priceDisplay;
    [SerializeField] private Button _leftArrow;
    [SerializeField] private Button _rightArrow;
    [SerializeField] private UIMainMenuNameDisplay _nameDisplay;

    private System.Action _upgradeButtonHandler = delegate { };
    private bool _shouldMonitorPosUpdate = false;
    private bool _isScrollViewUpdating = false;
    private float _scrollViewUpdateThreshold = 1;

    public bool ShouldMonitorPosUpdate
    {
        set { _shouldMonitorPosUpdate = value; }
    }

    public bool IsScrollViewUpdating
    {
        get { return _isScrollViewUpdating; }
    }

    public Button UpgradeButton
    {
        get { return _upgradeButton; }
    }

    public UIMainMenuPriceDisplay PriceDisplay
    {
        get { return _priceDisplay; }
    }

    public UIMainMenuNameDisplay NameDisplay
    {
        get { return _nameDisplay; }
    }

    public UICharacterMenuScrollView ScrollView
    {
        get { return _scrollView; }
    }

    public void AddUpgradeButtonHandler(System.Action handler)
    {
        _upgradeButtonHandler += handler;
    }

    private void Awake()
    {
        _upgradeButton.onClick.AddListener(UpgradeButtonPressHandler);
        _leftArrow.onClick.AddListener(LeftArrowHandler);
        _rightArrow.onClick.AddListener(RightArrowHandler);
        _scrollView.ScrollPositionController.OnPositionUpdating.AddListener(ScrollViewPositionUpdatingHandler);
        _scrollView.ScrollPositionController.OnBeginDrag.AddListener(ScrollViewBeginDragHandler);
        _scrollView.ScrollPositionController.OnEndDrag.AddListener(ScrollViewEndDragHandler);
        UpdateContent();
    }

    public void UpdateContent()
    {
        var cellDataList = new List<UICharacterMenuScrollViewData>();

        for (int i = 0; i < DataCenter.Instance.CharacterDataDic.Count; i++)
        {
            var charData = DataCenter.Instance.CharacterDataDic.ElementAt(i);
            var cellData = new UICharacterMenuScrollViewData()
            {
                ModelName = charData.Value.ModelName,
                PrefabPath = charData.Value.PrefabPath,
                Price = charData.Value.Price,
                DisplayAnimation = charData.Value.DisplayAnimation
            };
            cellDataList.Add(cellData);
        }

        _scrollView.UpdateData(cellDataList);
        _scrollView.UpdateContents();
        var index = cellDataList.IndexOf(cellDataList.Where(x => x.ModelName == GameSave.SelectedModel).First());
        UIController.Instance.UpdateSelectedCharacter(index);
    }

    private void ScrollViewPositionUpdatingHandler(bool updating)
    {
        _isScrollViewUpdating = updating;
        if (_shouldMonitorPosUpdate && updating == false)
        {
            if (_scrollViewUpdateThreshold > 0)
            {
                _scrollViewUpdateThreshold -= Time.deltaTime;
            }
            else
            {
                UIController.Instance.UpdateSelectedCharacter(_scrollView.SelectedIndex);
            }
        }
    }

    public void ScrollPositionUpdated()
    {
        _shouldMonitorPosUpdate = false;
        _scrollViewUpdateThreshold = 1;
    }

    private void ScrollViewBeginDragHandler()
    {
        _upgradeButton.gameObject.SetActive(false);
        _priceDisplay.gameObject.SetActive(false);
    }

    private void ScrollViewEndDragHandler()
    {
        _shouldMonitorPosUpdate = true;
    }

    private void UpgradeButtonPressHandler()
    {
        _upgradeButtonHandler();
    }

    private void LeftArrowHandler()
    {
        var index = Mathf.Clamp(_scrollView.SelectedIndex - 1, 0, _scrollView.ScrollPositionController.GetDataCount() - 1);
        _scrollView.MoveToIndex(index);
    }

    private void RightArrowHandler()
    {
        var index = Mathf.Clamp(_scrollView.SelectedIndex + 1, 0, _scrollView.ScrollPositionController.GetDataCount() - 1);
        Debug.Log("index: " + index);
        _scrollView.MoveToIndex(index);
    }
}