using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuPriceDisplay : MonoBehaviour
{
    [SerializeField] private Text _text;

    public void ShowPrice(int price)
    {
        _text.text = price.ToString();
    }
}