using UnityEngine;

public class UIResolutionSolver : MonoBehaviour
{
    [SerializeField] private GameObject iPadObject;
    [SerializeField] private GameObject iPhoneObject;
    [SerializeField] private bool show = true;

    private void OnEnable()
    {
        Show(show);
    }

    public void Show(bool value)
    {
        show = value;
        if (Camera.main.aspect < 1.5f)
        {
            iPadObject.SetActive(show);
            iPhoneObject.SetActive(!show);
        }
        else
        {
            iPadObject.SetActive(!show);
            iPhoneObject.SetActive(show);
        }
    }
}