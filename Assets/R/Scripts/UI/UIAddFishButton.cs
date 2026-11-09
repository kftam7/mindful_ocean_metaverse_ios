using ProGamer;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This button is shared by AddFish and Metaverse, with the purpose of avoiding
/// the addition of new child objects and a set of curves to the HiddenButton
/// animation file.
/// </summary>
public class UIAddFishButton : MonoBehaviour
{
    public Button Button;
    [SerializeField] private Image image;
    [SerializeField] private Text text;
    [SerializeField] private Sprite addFishSprite;
    [SerializeField] private Sprite metaverseSprite;

    public void Init()
    {
        bool isHatchery = GameSave.SelectedAquarium == GameSetting.HatcheryAquariumId;
        if (isHatchery)
        {
            image.sprite = addFishSprite;
            text.text = Localization.Get("txt_add_fish");
        }
        else
        {
            image.sprite = metaverseSprite;
            text.text = Localization.Get("txt_ar_metaverse");
        }
    }
}