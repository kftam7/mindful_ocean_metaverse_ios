using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIUpgradeDisplay : MonoBehaviour
{
    [SerializeField] private Text _valueText;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private Text _buttonText;
    public UnityEvent PurchaseButtonHandler;

    private void Awake()
    {
        _purchaseButton.onClick.AddListener(PurchaseButtonHandler.Invoke);
    }

    public void UpdateValueText(string val)
    {
        _valueText.text = val;
    }

    public void UpdateButtonText(string val)
    {
        _buttonText.text = val;
    }
}