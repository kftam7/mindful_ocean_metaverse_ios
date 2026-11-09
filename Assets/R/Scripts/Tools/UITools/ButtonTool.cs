using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTool : ProGamer.Singleton<ButtonTool>
{
    protected ButtonTool() { }

    public static void PausingInteractable(Button button, float time)
    {
        if (button != null)
        {
            button.interactable = false;
            Instance.StartCoroutine(Instance.PausingButtonInteractableEnumerator(button, time));
        }
    }

    public static void PausingInteractable(GameObject button, float time)
    {
        if (button != null && button.GetComponent<Button>() != null)
        {
            button.GetComponent<Button>().interactable = false;
            Instance.StartCoroutine(Instance.PausingButtonInteractableEnumerator(button.GetComponent<Button>(), time));
        }
    }

    IEnumerator PausingButtonInteractableEnumerator(Button button, float time)
    {
        yield return new WaitForSeconds(time);
        if (button != null) button.interactable = true;
    }
}