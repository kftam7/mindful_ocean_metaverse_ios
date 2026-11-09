using UnityEngine;
using UnityEngine.UI;

public class AquariumButton : MonoBehaviour
{
    public Text AquariumName;
    public Image AquariumPreview;
    public GameObject AddButtonContent;
    public GameObject DeleteButton;
    public bool IsAddButton;
    public AquariumData Data;
    public Button MainButton;

    protected bool showingDelete = false;

    void Start()
    {
        DeleteButton.SetActive(false);
        showingDelete = false;
    }

    public void SetData(AquariumData d)
    {
        if (d != null)
        {
            this.IsAddButton = false;
            this.Data = d;

            Sprite preview = null;

            if (!string.IsNullOrEmpty(d.SnapShotPath))
            {
                preview = ImageDataWriter.LoadSprite(d.SnapShotPath);
            }

            if (preview != null)
            {
                this.AquariumPreview.sprite = preview;

                Vector2 buttonSize = AquariumPreview.rectTransform.sizeDelta;
                this.AquariumPreview.rectTransform.sizeDelta = new Vector2(buttonSize.y / Screen.height * Screen.width, buttonSize.y);
            }
            else
            {
                this.AquariumPreview.sprite = SpriteDicCenter.Instance.ThemeSprite.GetSpriteByName(d.Data.Icon);
            }

            AquariumName.SetLocalization("txt_aquarium", "{0} " + d.Index.ToString("00"));

            /*if (d.Index > 0)
            {
                AquariumName.SetLocalization("txt_aquarium", "{0} " + d.Index.ToString("00"));
      
            }else{
      
                AquariumName.SetLocalization(d.AquariumName);
            }*/
        }
        else
        {
            this.IsAddButton = true;
            AquariumName.SetLocalization("txt_new_aquarium");
        }

        MainButton.onClick.AddListener(OnClick);

        AddButtonContent.SetActive(this.IsAddButton);
    }

    public void OnClick()
    {
        if (IsAddButton) { }
        else
        {
            GameSave.SelectedAquarium = Data.AquariumId;
            ProGamer.SoundManager.Instance.Play("ocean_saver_choose_aquarium_button_sfx");
            SceneTransitHandler.LoadScene("Aquarium");
        }
    }

    public void SetDeleteButtonActive(bool active)
    {
        if (!IsAddButton)
        {
            DeleteButton.SetActive(active);
            showingDelete = active;
        }
    }

    public void ToggleDelete()
    {
        SetDeleteButtonActive(!showingDelete);
    }
}