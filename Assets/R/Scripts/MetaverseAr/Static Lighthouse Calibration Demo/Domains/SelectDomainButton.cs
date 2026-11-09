using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectDomainButton : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Button helpButton;

    private Action _onSelect;
    private Action<bool> _onInvokeHelp;
    private bool _isCustomDomain;

    public void Init(string domainName, bool customDomain, Action onSelect, Action<bool> onInvokeHelp)
    {
        nameText.text = domainName;
        _isCustomDomain = customDomain;
        _onSelect = onSelect;
        _onInvokeHelp = onInvokeHelp;

        // if (domainName.Equals("Ocean Race", StringComparison.CurrentCultureIgnoreCase))
        //     GetComponent<Button>().interactable = false;

        helpButton.onClick.AddListener(() => _onInvokeHelp?.Invoke(!_isCustomDomain));
    }

    public void OnClick()
    {
        _onSelect?.Invoke();
    }
}
