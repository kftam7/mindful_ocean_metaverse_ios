using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UICharacterMenuScrollViewCell : FancyScrollViewCell<UICharacterMenuScrollViewData, UICharacterMenuScrollViewContext>
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private Button _buttonComponent;
    private UICharacterMenuScrollViewContext _context;
    private GameObject _prefab = null;
    private GameObject _model = null;
    readonly int scrollTriggerHash = Animator.StringToHash("scroll");
    private UICharacterMenuScrollViewData data;

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnModelPurchased, OnModelPurchased);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnModelPurchased, OnModelPurchased);
    }

    protected void OnModelPurchased(object result)
    {
        string model = (string)result;

        Debug.Log("Purchased " + model);
        if (data != null && data.ModelName == model)
        {
            UpdateContent(data);
        }
    }

    private void Awake()
    {
        _buttonComponent.onClick.AddListener(() =>
        {
            this._context.OnPressedCell.Invoke(this);
        });
    }

    public override void SetContext(UICharacterMenuScrollViewContext context)
    {
        this._context = context;
    }

    public override void UpdateContent(UICharacterMenuScrollViewData itemData)
    {
        this.data = itemData;

        // should show lock?
        var locked = !DataCenter.Instance.GetAvailabilityOfModel(itemData.ModelName) && itemData.Price > 0;

#if EVENT_DEMO
		locked = false;
#endif

        _lockIcon.gameObject.SetActive(locked);
        // update Model
        var tmpPrefab = Resources.Load<GameObject>(itemData.PrefabPath);
        if (tmpPrefab != _prefab)
        {
            _prefab = tmpPrefab;
            LoadModel(itemData.DisplayAnimation);
        }

        // Change skin to 
        Debug.Log("Loading char menu cell with skin " + GameSave.GetModelSkin(itemData.ModelName));
        if (!string.IsNullOrEmpty(GameSave.GetModelSkin(itemData.ModelName)))
        {
            var texture = ImageDataWriter.LoadImage(GameSave.GetModelSkin(itemData.ModelName), true);
            _model.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", texture);
        }
        else
        {
            // reload the prefab because the we want to load the default skin
            LoadModel(itemData.DisplayAnimation);
        }
    }

    private void LoadModel(string animationName)
    {
        Destroy(_model);
        _model = GameObject.Instantiate(_prefab, this.transform);
        _model.transform.localScale = 450 * Vector3.one;
        var animator = _model.GetComponent<Animator>();
        animator.Play(animationName);
    }

    public override void UpdatePosition(float position)
    {
        _animator.Play(scrollTriggerHash, -1, position);
        _animator.speed = 0;
    }
}