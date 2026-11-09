using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxView : BaseView<DialogObject>
{
    [SerializeField] private Text messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button noButton;

    private DialogObject _currentData;
    private RectTransform _mainPanelTransform;

    void Start()
    {
        okButton.onClick.AddListener(YesCallback);
        noButton.onClick.AddListener(NoCallback);

        _mainPanelTransform = mainPanel.GetComponent<RectTransform>();
    }

    public override void Show(DialogObject data)
    {
        PrepareDialog(data);
        base.Show(data);
    }

    private void PrepareDialog(DialogObject data)
    {
        _currentData = data;
        messageText.SetLocalization(_currentData.key);
        noButton.gameObject.SetActive(!_currentData.noDialog);

        if (!string.IsNullOrEmpty(_currentData.yesKey))
            okButton.GetComponentInChildren<Text>().SetLocalization(_currentData.yesKey);
        else
            okButton.GetComponentInChildren<Text>().SetLocalization("txt_ok");

        if (!string.IsNullOrEmpty(_currentData.noKey))
            noButton.GetComponentInChildren<Text>().SetLocalization(_currentData.noKey);

        _mainPanelTransform.RefreshLayoutGroupsImmediateAndRecursive();
    }

    private void YesCallback()
    {
        _currentData?.yesAction?.Invoke();
        Hide();
    }

    private void NoCallback()
    {
        _currentData?.noAction?.Invoke();
        Hide();
    }
}

public class DialogObject
{
    public string key;
    public string yesKey;
    public Action yesAction;
    public string noKey;
    public Action noAction;
    public bool noDialog;
}