using UnityEngine;
using UnityEngine.UI;

public class UIHideToolsToggle : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject[] tools;

    private void OnEnable()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        GetComponent<Toggle>().onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        background.SetActive(!value);
        foreach (GameObject tool in tools)
        {
            tool.SetActive(value);
        }
    }
}