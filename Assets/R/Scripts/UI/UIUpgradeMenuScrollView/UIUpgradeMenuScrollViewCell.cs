using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIUpgradeMenuScrollViewCell : FancyScrollViewCell<UIUpgradeMenuScrollViewData, UIUpgradeMenuScrollViewContext>
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Button _buttonComponent;
    [SerializeField] private Button _deleteButton;
    private UIUpgradeMenuScrollViewContext _context;
    private GameObject _prefab = null;
    private GameObject _model = null;

    public UIUpgradeMenuScrollViewContext Context
    {
        get { return _context; }
    }

    public Texture ModelTexture
    {
        get { return _model.GetComponentInChildren<Renderer>().material.mainTexture; }
    }

    protected UIUpgradeMenuScrollViewData cellData;

    readonly int scrollTriggerHash = Animator.StringToHash("scroll");

    private void Awake()
    {
        _buttonComponent.onClick.AddListener(() =>
        {
            this._context.OnPressedCell.Invoke(this);
        });

        _deleteButton.onClick.AddListener(() =>
        {
            this._context.OnDeletePress.Invoke(this);
        });
    }

    void OnEnable()
    {
        OnSkinEditModeStatusChanged(UIUpgradeMenuScrollView.IsEditModeOn);
        EventManager.StartListening(EventList.OnSkinEditModeStatusChanged, OnSkinEditModeStatusChanged);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnSkinEditModeStatusChanged, OnSkinEditModeStatusChanged);
    }

    protected void OnSkinEditModeStatusChanged(object result)
    {
        bool active = (bool)result;
        _deleteButton.gameObject.SetActive(active && !cellData.IsDefault);
    }

    public override void SetContext(UIUpgradeMenuScrollViewContext context)
    {
        this._context = context;
    }

    public override void UpdateContent(UIUpgradeMenuScrollViewData itemData)
    {
        var tmpPrefab = Resources.Load<GameObject>("Models/Characters/" + itemData.ImageData.GetModelName());
        if (tmpPrefab != _prefab || itemData.IsDefault)
        {
            _prefab = tmpPrefab;
            Destroy(_model);
            _model = GameObject.Instantiate(_prefab, this.transform);
            _model.transform.localScale = 100 * Vector3.one;
        }

        //            if (!itemData.IsDefault)
        if (!itemData.IsDefault)
        {
            if (cellData == null || cellData.ImageData.ImageFile != itemData.ImageData.ImageFile)
            {
                string smallFileName = ImageFileNameUtils.GetSmallImageFullFileName(itemData.ImageData.ImageFile);
                var texture = TextureCache.Instance.Load(smallFileName, true);
                _model.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", texture);
            }
        }

        cellData = itemData;
    }

    public override void UpdatePosition(float position)
    {
        _animator.Play(scrollTriggerHash, -1, position);
        _animator.speed = 0;
    }
}