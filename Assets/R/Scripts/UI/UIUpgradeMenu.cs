using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradeMenu : MonoBehaviour
{
    [SerializeField] private UIUpgradeMenuScrollView _scrollView;
    [SerializeField] private Button _backButton;
    [SerializeField] private Transform _characterRootTransform;
    [SerializeField] private UIUpgradeDisplay _hpUpgradeDisplay;
    [SerializeField] private UIUpgradeDisplay _bonusUpgradeDisplay;
    [SerializeField] private UIMainMenuNameDisplay _nameDisplay;
    [SerializeField] private Button _nameEditButton;
    private bool _isNameEditable = false;
    private CharacterData _charData;
    private GameObject _characterModelGO;
    private List<UIUpgradeMenuScrollViewData> _cellDataList = new List<UIUpgradeMenuScrollViewData>();

    private System.Action _backButtonHandler = delegate { };

    public void AddBackButtonHandler(System.Action handler)
    {
        _backButtonHandler = handler;
    }

    private void Awake()
    {
        _backButton.onClick.AddListener(BackButtonPressHandler);
        _nameEditButton.onClick.AddListener(NameEditHandler);
        _nameDisplay.AddOnValueChangeHandler(NameChangedHandler);
        _hpUpgradeDisplay.PurchaseButtonHandler.AddListener(UpgradeHPHandler);
        _bonusUpgradeDisplay.PurchaseButtonHandler.AddListener(UpgradeBonusHandler);
        _scrollView.AddCellPressedHandler(ScrollViewCellPressedHandler);
        _scrollView.AddDeletePressedhandler(ScrollViewDeletePressedHandler);
    }

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnSkinDeleted, OnSkinDeleted);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnSkinDeleted, OnSkinDeleted);
    }

    public void OnSkinDeleted(object result)
    {
        UpdateContent(DataCenter.Instance.CharacterDataDic[GameSave.SelectedModel]);
    }

    public void UpdateContent(CharacterData charData)
    {
        _charData = charData;
        // Load Icon
        if (_characterModelGO != null) Destroy(_characterModelGO);
        var prefab = Resources.Load<GameObject>(charData.PrefabPath);
        _characterModelGO = Instantiate(prefab, _characterRootTransform);
        _characterModelGO.transform.localScale = 450 * Vector3.one;

        string skin = GameSave.GetModelSkin(GameSave.SelectedModel);

        if (!string.IsNullOrEmpty(skin))
        {
            var texture = ImageDataWriter.LoadImage(skin, true);
            _characterModelGO.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", texture);
        }

        var upgradeData = DataCenter.Instance.GetCharacterUpgradeData(charData.ModelName);
        // Load HP
        _hpUpgradeDisplay.UpdateValueText(upgradeData.GetHP().ToString());
        _hpUpgradeDisplay.UpdateButtonText(upgradeData.GetHPUpgradeCost().ToString());
        // Load Bonus
        _bonusUpgradeDisplay.UpdateValueText(upgradeData.GetBonus().ToString("F2"));
        _bonusUpgradeDisplay.UpdateButtonText(upgradeData.GetBonusUpgradeCost().ToString());
        // Load skins
        var imageDataList = (from d in ImageDataManager.Instance.ImageDataList where d.GetModelName() == charData.ModelName select d).ToList();
        var defaultImage = new ImageData("Textures/" + charData.ModelName);
        imageDataList.Insert(0, defaultImage);
        _cellDataList = imageDataList.ConvertAll(imgData => new UIUpgradeMenuScrollViewData()
        {
            ImageData = imgData,
            IsDefault = imgData.ImageFile == defaultImage.ImageFile
        });

        // for testing the delete function
        // _cellDataList.Clear();
        // for (int i = 0; i < 4; i++)
        // {
        // 	_cellDataList.Add(new UIUpgradeMenuScrollViewData()
        // 	{
        // 		ImageData = new ImageData("aaa", "Shark"),
        // 		IsDefault = false
        // 	});
        // }
        // _cellDataList.Insert(0, new UIUpgradeMenuScrollViewData()
        // {
        // 	ImageData = new ImageData("Textures/" + charData.ModelName, charData.ModelName),
        // 	IsDefault = true
        // });

        _scrollView.UpdateData(_cellDataList);
        _scrollView.SelectedIndex = 0;
        _scrollView.UpdateContents();
        _isNameEditable = true;
    }

    private void BackButtonPressHandler()
    {
        _backButtonHandler();
        _nameDisplay.Interactable = false;
        _isNameEditable = false;
        _charData = null;
    }

    private void ScrollViewCellPressedHandler(UIUpgradeMenuScrollViewCell cell)
    {
        // 0 is default
        string skin = cell.DataIndex == 0 ? string.Empty : _cellDataList[cell.DataIndex].ImageData.ImageFile;
        GameSave.SetModelSkin(_charData.ModelName, skin);
        // apply skin to icon model
        // var texture = string.IsNullOrEmpty(GameSave.SelectedSkin) ? Resources.Load<Texture>("Textures/" + _charData.ModelName) :
        // 	ImageDataManager.Instance.LoadImage(GameSave.SelectedSkin);
        _characterModelGO.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", cell.ModelTexture);
    }

    private void ScrollViewDeletePressedHandler(int cellIndex)
    {
        var removedImageData = _cellDataList[cellIndex].ImageData;
        ImageDataManager.Instance.RemoveImageData(removedImageData, true, true, true);
        _cellDataList.RemoveAt(cellIndex);
        _scrollView.UpdateData(_cellDataList);
    }

    private void UpgradeHPHandler()
    {
        DataCenter.Instance.UpgradeHP(_charData.ModelName);
        var upgradeData = DataCenter.Instance.GetCharacterUpgradeData(_charData.ModelName);
        _hpUpgradeDisplay.UpdateValueText(upgradeData.GetHP().ToString());
        _hpUpgradeDisplay.UpdateButtonText(upgradeData.GetHPUpgradeCost().ToString());
    }

    private void UpgradeBonusHandler()
    {
        DataCenter.Instance.UpgradeBonus(_charData.ModelName);
        var upgradeData = DataCenter.Instance.GetCharacterUpgradeData(_charData.ModelName);
        _bonusUpgradeDisplay.UpdateValueText(upgradeData.GetBonus().ToString("F2"));
        _bonusUpgradeDisplay.UpdateButtonText(upgradeData.GetBonusUpgradeCost().ToString());
    }

    private void NameEditHandler()
    {
        _nameDisplay.Interactable = !_nameDisplay.Interactable;
        if (_nameDisplay.Interactable)
        {
            _nameDisplay.SetToFocus();
        }
    }

    private void NameChangedHandler(string name)
    {
        if (_charData != null && _isNameEditable)
        {
            if (name != _charData.ModelName)
                GameSave.SetEditedName(_charData.ModelName, name);
        }
    }
}