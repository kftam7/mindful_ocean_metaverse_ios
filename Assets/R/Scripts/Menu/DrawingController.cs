using UnityEngine;
using UnityEngine.UI;
using PaintCraft.Tools;
using PaintCraft.Controllers;
using PaintCraft.Canvas;
using PaintCraft.Canvas.Configs;
using System;
using System.Collections;
using PatinCraft.UI;
using GoAGo;
using ProGamer;

public class DrawingController : MonoBehaviour
{
    public CanvasController DrawingCanvasController;
    public LineConfig DrawingLineConfig;

    public GameObject ColorPicker;
    public Image[] ColorFollower;
    public Toggle[] BrushSizeToggle;
    public Image[] colorButtonImages;

    public GameObject Tools;

    [SerializeField] protected GameObject SizeNumberGO;
    [SerializeField] protected GameObject ColourGO;
    [SerializeField] protected GameObject SizeGO;
    [SerializeField] protected GameObject BrightnessGO;
    [SerializeField] protected Button prevColorSetButton;
    [SerializeField] protected Button nextColorSetButton;
    [SerializeField] protected Toggle[] BrushToggle;
    [SerializeField] protected Slider brightnessSlider;
    [SerializeField] protected DrawingColorSet[] colorSets;
    [SerializeField] protected ChangeBrushOnClickController brushOnClickController;
    [SerializeField] protected Image brightnessImage;
    [SerializeField] protected Sprite brightnessImageTextureRef;
    [SerializeField] protected Button backButton;

    private void Awake()
    {
        //Calculate Region Map here
        AdvanceColoringPageConfig config = AppData.SelectedPageConfig as AdvanceColoringPageConfig;
        if (config != null)
        {
            config.GenerateRegionMapByOutline();
            DrawingCanvasController.RegionTexture = config.RegionTexture;
        }
    }

    private void Start()
    {
        //Color lastUsingColor = GameSave.SavedPickerColor;
        Analytices.Instance.EnterScreen(AnalyticsEnums.ValueScreenDrawingBoard);

        backButton.onClick.AddListener(backButtonOnClick);

        Toggle[] ts = ColorPicker.GetComponentsInChildren<Toggle>();

        bool hasLastColor = false;

        //Set color button to saved color set
        for (int i = 0; i < colorButtonImages.Length; ++i)
        {
            colorButtonImages[i].color = GetColor(GameSave.SavedPickerColorSetIndex, i);
            int j = i;
            Toggle colorButtonToggle = colorButtonImages[i].GetComponent<Toggle>();
            colorButtonToggle.onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    ColorPicked(j);
                }
            });
            colorButtonToggle.isOn = GameSave.SavedPickerColorIndex == i;
        }

        Debug.Log("SavedPickerColorIndex: " + GameSave.SavedPickerColorIndex);
        ColorPicked(GameSave.SavedPickerColorIndex);
        UpdateColorSetButton();

        brightnessSlider.value = GameSave.LastColorBrightness;
        DrawingLineConfig.Color.Color = GetDrawingColorWithBrightness();

        float lastSize = GameSave.LastBrushSize;

        bool hasLastSize = false;

        for (int i = 0; i < BrushSizeToggle.Length; i++)
        {
            float size = (1f + i) / BrushSizeToggle.Length;

            if (BrushSizeToggle[i].isOn = lastSize.EqualCompare(size))
            {
                hasLastSize = true;
            }

            BrushSizeToggle[i].onValueChanged.AddListener((bool isOn) =>
            {
                SetBrushSize(size);
            });
        }

        if (!hasLastSize && BrushSizeToggle.Length > 2)
        {
            BrushSizeToggle[2].isOn = true;
        }

        foreach (var t in Tools.GetComponentsInChildren<ToggleAnimation>())
        {
            t.GetComponent<Toggle>().animator.SetTrigger(t.GetComponent<Toggle>().animationTriggers.normalTrigger);
            t.OnAnimation();
        }

        StartCoroutine(LateStart(0.1f));

        if (GameSave.ShowTutorialColoring)
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.TutorialMessagePopup, true);
            var popup = PopupManager.Instance.ActivePopup as TutorialMessagePopup;
            popup.SetTutorial(TutorialType.Coloring);
        }
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //Your Function You Want to Call
        foreach (var t in Tools.GetComponentsInChildren<ToggleAnimation>())
        {
            if (t.TargetToggle.isOn)
            {
                t.GetComponent<Toggle>().animator.SetTrigger(t.GetComponent<Toggle>().animationTriggers.highlightedTrigger);
                t.OnAnimation();
            }
        }
    }

    public void ShowModelPreview()
    {
        NativeHelper.GenerateImpactFeedback();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ModelPreviewPopup);
        var popup = PopupManager.Instance.ActivePopup as ModelPreviewPopup;
    }

    public void backButtonOnClick()
    {
        ButtonTool.PausingInteractable(backButton, 0.3f);
        // there are other function being called on scene's button component's onclick. ToDo: move back to here
    }

    public void ColorPicked(int colorIndex)
    {
        GameSave.SavedPickerColorIndex = colorIndex;
        DrawingLineConfig.Color.Color = GetDrawingColorWithBrightness();
        foreach (Image image in ColorFollower)
        {
            image.color = GetDrawingColorWithBrightness();
        }

        Debug.Log("ColorPicked: " + colorIndex);
        UpdateBrightnessImage();
        brightnessSlider.value = ColorUtils.RGBtoHSL(GetSavedColor()).l;
        NativeHelper.GenerateImpactFeedback();
    }

    public void PreviousColorSet()
    {
        NativeHelper.GenerateImpactFeedback();
        GameSave.SavedPickerColorSetIndex -= 1;
        for (int i = 0; i < colorButtonImages.Length; ++i)
        {
            colorButtonImages[i].color = GetColor(GameSave.SavedPickerColorSetIndex, i);
        }

        UpdateColorSetButton();
        ColorPicked(GameSave.SavedPickerColorIndex);
    }

    public void NextColorSet()
    {
        NativeHelper.GenerateImpactFeedback();
        GameSave.SavedPickerColorSetIndex += 1;
        for (int i = 0; i < colorButtonImages.Length; ++i)
        {
            colorButtonImages[i].color = GetColor(GameSave.SavedPickerColorSetIndex, i);
        }

        UpdateColorSetButton();
        ColorPicked(GameSave.SavedPickerColorIndex);
    }

    public void OnBrushChanged(string brushName)
    {
        try
        {
            if (brushName.Contains("Rubber") && ColourGO != null && BrightnessGO != null && SizeGO != null)
            {
                ColourGO.SetActive(false);
                BrightnessGO.SetActive(false);
                SizeGO.SetActive(true);
                NativeHelper.GenerateImpactFeedback();
            }
            else if (brushName.Contains("Brush") && ColourGO != null && BrightnessGO != null && SizeGO != null)
            {
                ColourGO.SetActive(true);
                BrightnessGO.SetActive(true);
                SizeGO.SetActive(true);
                NativeHelper.GenerateImpactFeedback();
            }
            else if (brushName.Contains("Spray") && ColourGO != null && BrightnessGO != null && SizeGO != null)
            {
                ColourGO.SetActive(true);
                BrightnessGO.SetActive(true);
                SizeGO.SetActive(true);
                NativeHelper.GenerateImpactFeedback();
            }
            else if (brushName.Contains("Marker") && ColourGO != null && BrightnessGO != null && SizeGO != null)
            {
                ColourGO.SetActive(true);
                BrightnessGO.SetActive(true);
                SizeGO.SetActive(true);
                NativeHelper.GenerateImpactFeedback();
            }
            else if (brushName.Contains("Bucket") && ColourGO != null && BrightnessGO != null && SizeGO != null)
            {
                ColourGO.SetActive(true);
                BrightnessGO.SetActive(true);
                SizeGO.SetActive(false);
                NativeHelper.GenerateImpactFeedback();
            }
            else if (brushName.Contains("Crayon") && ColourGO != null && BrightnessGO != null && SizeGO != null)
            {
                ColourGO.SetActive(true);
                BrightnessGO.SetActive(true);
                SizeGO.SetActive(false);
                NativeHelper.GenerateImpactFeedback();
            }

            foreach (var t in Tools.GetComponentsInChildren<ToggleAnimation>())
            {
                if (!t.TargetToggle.isOn)
                {
                    t.GetComponent<Toggle>().animator.SetTrigger(t.GetComponent<Toggle>().animationTriggers.normalTrigger);
                    t.OnAnimation();
                }
            }
        }
        catch (Exception e)
        {
            // it's showing "Trying to remove xxx from rebuild list while we are already inside a rebuild loop." too frequrently
        }
    }

    public void SetBrushSize(float size)
    {
        DrawingLineConfig.Scale = size;

        if (showingThread != null)
        {
            StopCoroutine(showingThread);
        }

        showingThread = ShowSizeNumber();
        StartCoroutine(showingThread);
        NativeHelper.GenerateImpactFeedback();
    }

    public void SetBrightness(float brightness)
    {
        GameSave.LastColorBrightness = brightness;
        DrawingLineConfig.Color.Color = GetDrawingColorWithBrightness();
        foreach (Image image in ColorFollower)
        {
            image.color = GetDrawingColorWithBrightness();
        }
        // haptic too noisy for this one
        //NativeHelper.GenerateImpactFeedback();

        //if (showingThread != null)
        //{
        //    StopCoroutine(showingThread);
        //}
        //showingThread = ShowSizeNumber();
        //StartCoroutine(showingThread);
    }

    IEnumerator showingThread;

    IEnumerator ShowSizeNumber()
    {
        if (SizeNumberGO != null)
        {
            SizeNumberGO.SetActive(true);
            SizeNumberGO.GetComponentInChildren<Text>().text = DrawingLineConfig.Scale.ToString();
            yield return new WaitForSeconds(2f);
            SizeNumberGO.SetActive(false);
        }
    }

    public void Back(bool showSavedMessage = true)
    {
        bool isNewImage = false;
        GameSave.SelectedAquarium = DataCenter.Instance.GetAquariumIdByName(GameSetting.HatcheryAquariumName);
        if (ImageDataManager.Instance.NewImageData != null && !ImageDataManager.Instance.NewImageData.Submitted)
        {
            ImageDataManager.Instance.AddNewImageDataIntoAquarium(true, false); // confirm to add the new fish into the aquarium
            ImageDataManager.Instance.NewImageData = null;
            GameSave.NewFishAddedFlag = true;
            isNewImage = true;
        }

        if (showSavedMessage && (isNewImage || DrawingCanvasController.UndoManager.HasUndo()))
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.HintPopup, true);
            var popup = PopupManager.Instance.ActivePopup as HintPopup;
            popup.SetTextKeyLower("txt_artwork_saved");
            popup.SetDelayClose(1, () =>
            {
                SceneTransitHandler.LoadScene("Aquarium");
            });
        }
        else
        {
            SceneTransitHandler.LoadScene("Aquarium");
        }
    }

    public void EnableRegion()
    {
        ColoringPageConfig config = AppData.SelectedPageConfig as ColoringPageConfig;

        if (config != null)
        {
            DrawingCanvasController.RegionTexture = config.RegionTexture;
        }
    }

    public void DisableRegion()
    {
        ColoringPageConfig config = AppData.SelectedPageConfig as ColoringPageConfig;

        if (config != null)
        {
            DrawingCanvasController.RegionTexture = null;
        }
    }

    private void OnDestroy()
    {
        Analytices.Instance.LeaveScreen(AnalyticsEnums.ValueScreenDrawingBoard);
        GL.Clear(true, true, Color.black);
    }

    private float GetColorBrightness(Color color)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        return v;
    }

    private Color GetDrawingColorWithBrightness()
    {
        ColorHSL colorHSL = ColorUtils.RGBtoHSL(GetSavedColor());
        colorHSL.l = GameSave.LastColorBrightness;
        return ColorUtils.HSLtoRGB(colorHSL);
    }

    private Color GetSavedColor()
    {
        return GetColor(GameSave.SavedPickerColorSetIndex, GameSave.SavedPickerColorIndex);
    }

    private Color GetColor(int colorSetIndex, int colorIndex)
    {
        return colorSets[colorSetIndex].GetColor(colorIndex);
    }

    private void UpdateColorSetButton()
    {
        prevColorSetButton.interactable = true;
        nextColorSetButton.interactable = true;

        if (GameSave.SavedPickerColorSetIndex == 0)
        {
            prevColorSetButton.interactable = false;
        }
        else if (GameSave.SavedPickerColorSetIndex == colorSets.Length - 1)
        {
            nextColorSetButton.interactable = false;
        }
    }

    private void UpdateBrightnessImage()
    {
        var texture = new Texture2D(brightnessImageTextureRef.texture.width, brightnessImageTextureRef.texture.height, TextureFormat.RGBA32, false);
        //

        ColorHSL colorHSL = ColorUtils.RGBtoHSL(GetSavedColor());
        for (int i = 0; i < texture.width; ++i)
        {
            for (int j = 0; j < texture.height; ++j)
            {
                Color color = ColorUtils.HSLtoRGB(new ColorHSL(colorHSL.h, colorHSL.s, (float)i / texture.width));
                color.a = brightnessImageTextureRef.texture.GetPixel(i, j).a;
                texture.SetPixel(i, j, color);
            }
        }

        Debug.Log(GetSavedColor() + " -> " + ColorUtils.HSLtoRGB(colorHSL));
        texture.Apply();
        //brightnessImage.sprite = Sprite.Create(texture, brightnessImageTextureRef.rect, new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect, brightnessImageTextureRef.border);
        brightnessImage.sprite = Sprite.Create(texture, brightnessImageTextureRef.rect, new Vector2(0.5f, 0.5f));
    }
}