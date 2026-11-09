using UnityEngine;
using UnityEngine.UI;

public class StartView : BaseView<StartViewOption>
{
    [SerializeField] private GameObject background;
    [SerializeField] private Text messageText;

    public override void Show(StartViewOption data)
    {
        // messageText.text = data.message;
        background.SetActive(data.showBg);
        base.Show(data);
    }

    public override void Hide()
    {
        background.SetActive(false);
        base.Hide();
    }
}

public class StartViewOption
{
    public string message;
    public bool showBg;
}