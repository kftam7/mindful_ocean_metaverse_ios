using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Auki.ConjureKit.Manna;
using ProGamer;
using Unity.VisualScripting;
using UnityEngine;

public class SelectDomainPopup : SimplePopup
{
    [SerializeField] SelectDomainButton _selectDomainButton;
    [SerializeField] Transform _content;
    [SerializeField] SelectDomainHelp selectDomainHelp;

    private List<SelectDomainButton> _selectDomainButtons = new List<SelectDomainButton>();
    private Action<LighthousePose> m_Callback;

    private readonly string[] _builtinDomains = new string[] { "Little Adventure", "Ocean Race" };
    private readonly Dictionary<string, string> _builtInDomainsKey = new Dictionary<string, string>()
    {
        {"Little Adventure", "txt_domain_option1"},
        {"Ocean Race", "txt_domain_option2"},
    };

    public void Init(string shortId, List<LighthousePose> domains, Action<LighthousePose> callback, Action customDomainCallback)
    {
        m_Callback = callback;
        gameObject.SetActive(true);

        foreach (var oldDomainButton in _selectDomainButtons)
        {
            Destroy(oldDomainButton.gameObject);
        }

        _selectDomainButtons.Clear();

        var orderedList = domains.OrderBy(s =>
        {
            if (s.domainName.Equals("Little Adventure", StringComparison.CurrentCultureIgnoreCase)) return 1;
            if (s.domainName.Equals("Ocean Race", StringComparison.CurrentCultureIgnoreCase)) return 2;
            return 3; // All other items
        }).ThenBy(x => x).ToList();

        var isBuiltInDomain = shortId.Equals(ARMetaverseManager.Instance.GetBuiltInQr(), StringComparison.CurrentCultureIgnoreCase);

        foreach (LighthousePose lighthousePose in orderedList)
        {
            var button = Instantiate(_selectDomainButton, _content);
            button.gameObject.SetActive(true);
            var isCustomDomain = !_builtinDomains.Contains(lighthousePose.domainName);
            var buttonCaption = isBuiltInDomain ? Localization.Get(_builtInDomainsKey[lighthousePose.domainName]) : lighthousePose.domainName;
            button.Init(buttonCaption, isCustomDomain, () => OnSelect(lighthousePose), selectDomainHelp.ShowHelp);
            _selectDomainButtons.Add(button);
        }

        if (isBuiltInDomain)
        {
            var button = Instantiate(_selectDomainButton, _content);
            button.gameObject.SetActive(true);
            button.Init(Localization.Get("txt_domain_option3"), true, () =>
            {
                Analytices.Instance.CustomDomainEvent("Your own domain", "", GameController.PlayerData.appleUserId);
                customDomainCallback?.Invoke();
                Close();
            }, selectDomainHelp.ShowHelp);
            _selectDomainButtons.Add(button);
        }

        if (GameSave.ShowTutorialDomainAdventureAr && isBuiltInDomain)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMessagePopup, true);
            var popup = PopupManager.Instance.ActivePopup as TutorialMessagePopup;
            popup.SetTutorial(TutorialType.DomainAdventure);
        }
    }

    private void OnSelect(LighthousePose lighthousePose)
    {
        m_Callback?.Invoke(lighthousePose);
        Close();
    }
}
