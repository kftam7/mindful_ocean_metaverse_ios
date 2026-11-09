using UnityEngine;

public abstract class BaseView<T> : MonoBehaviour
{
    [SerializeField] protected GameObject mainPanel;

    public virtual void Show(T data)
    {
        mainPanel.SetActive(true);
    }

    public virtual void Hide()
    {
        mainPanel.SetActive(false);
    }
}