
using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class SelectDomainHelp : MonoBehaviour
{
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private Text helpText;
    [SerializeField] private Button setupButton;
    [SerializeField] private Button okButton;

    private Text _okBtnText, _setupBtnText;

    private const string BuiltInDomainHelpStringKey = "txt_builtin_domain_help";
    private const string CustomDomainHelpStringKey = "txt_custom_domain_help";
    private const string OkButtonStringKey = "txt_ok";
    private const string BuiltInSetupButtonStringKey = "txt_builtin_domain_setup_button";
    private const string CustomSetupButtonStringKey = "txt_custom_domain_setup_button";
    private const string SetupDomainLink = "https://www.mindful-ocean.org/faq-domain";

    void Awake()
    {
        _setupBtnText = setupButton.GetComponentInChildren<Text>();
        _okBtnText = okButton.GetComponentInChildren<Text>();

        okButton.onClick.AddListener(Hide);    
    }

    public void ShowHelp(bool builtInDomain)
    {
        var key = builtInDomain ? BuiltInDomainHelpStringKey : CustomDomainHelpStringKey;
        Localization.SetText(key, helpText);
        if (!_setupBtnText) _setupBtnText = setupButton.GetComponentInChildren<Text>();

        var setupBtnKey = builtInDomain ? BuiltInSetupButtonStringKey : CustomSetupButtonStringKey;
        Localization.SetText(setupBtnKey, _setupBtnText);

        setupButton.onClick.RemoveAllListeners();
        setupButton.onClick.AddListener(builtInDomain ? OpenBuiltInSetup : OpenCustomSetup);

        if (!_okBtnText) _okBtnText = okButton.GetComponentInChildren<Text>();
        Localization.SetText(OkButtonStringKey, _okBtnText);

        helpPanel.SetActive(true);
    }

    private void Hide()
    {
        helpPanel.SetActive(false);
    }

    private void OpenBuiltInSetup()
    {
        Analytices.Instance.LogSetupDomainEvent();
        Application.OpenURL(SetupDomainLink);
    }

    private void OpenCustomSetup()
    {
        Analytices.Instance.LogSetupDomainEvent();
        Application.OpenURL(SetupDomainLink);
    }
}