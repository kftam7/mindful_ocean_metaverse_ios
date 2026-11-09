using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;
using System.Text;
using UnityEngine.Purchasing;
using ProGamer;
using PaintCraft.Canvas;
using PaintCraft.Canvas.Configs;

public class PrintingUIManager : SceneInstance<PrintingUIManager>
{
    protected enum MenuStatus
    {
        InitState,
        PrintingPackageMenu,
        ColoringPageMenu
    }

    public ScrollRect PrintingPackageMenu;
    public ScrollRect PrintingMaterialMenu;
    public Toggle SelectionButton;

    public GameObject SaveButton;
    public GameObject PrintButton;
    public GameObject PurchaseButton;
    public GameObject drawButton;
    public GameObject backButton;
    public GameObject okButton;

    public delegate void LeaveSceneCallback();

    [SerializeField] protected PackageScrollView packageScrollView;
    [SerializeField] protected GameObject scanButton;
    [SerializeField] protected Text Title;

    protected List<GameObject> selectionList = new List<GameObject>();
    [SerializeField] protected List<PrintingMaterialData> selectedImage = new List<PrintingMaterialData>();

    protected MenuStatus currentStatus = MenuStatus.InitState;

    void Start()
    {
        SaveButton.GetComponent<Button>().onClick.AddListener(SaveToCameraRoll);
        PrintButton.GetComponent<Button>().onClick.AddListener(PrintColoringPaper);
        drawButton.GetComponent<Button>().onClick.AddListener(DrawButtonClick);
        scanButton.GetComponent<Button>().onClick.AddListener(ScanButtonClick);
        backButton.GetComponent<Button>().onClick.AddListener(Back);

        SelectionButton.onValueChanged.AddListener(SetSelectionMode);


        string lastShownPackage = GameSave.LastShownPackage;

        if (GameSave.ColoringMode == 0 && Title != null)
        {
            Title.text = Localization.Get("txt_in_app_coloring");
        }
        else if (GameSave.ColoringMode == 1 && Title != null)
        {
            Title.text = Localization.Get("txt_page_coloring");
        }

        Localization.SetFont(Title);

        if (SceneTransitHandler.PreviousScene == "ColoringBook" && !string.IsNullOrEmpty(lastShownPackage))
        {
            SwitchMenu(MenuStatus.ColoringPageMenu);
            ListPrintingMaterial(lastShownPackage, GameSave.LastSelectedColoringPageCellIndex);
        }
        else if (SceneTransitHandler.PreviousScene == "PreviewScene" && !string.IsNullOrEmpty(lastShownPackage))
        {
            SwitchMenu(MenuStatus.ColoringPageMenu);
            ListPrintingMaterial(lastShownPackage, GameSave.LastSelectedColoringPageCellIndex);
        }
        else
        {
            SwitchMenu(MenuStatus.PrintingPackageMenu);
        }
    }

    void OnEnable()
    {
        EventManager.StartListening(EventList.OnPurchaseSuccess, OnPurchaseSuccess);
        EventManager.StartListening(EventList.OnPurchaseFail, OnPurchaseFail);
        PrintingManager.Instance.OnPrintingProcessDone += OnPrintingDone;
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnPurchaseSuccess, OnPurchaseSuccess);
        EventManager.StopListening(EventList.OnPurchaseFail, OnPurchaseFail);
        PrintingManager.Instance.OnPrintingProcessDone -= OnPrintingDone;
    }

    void OnPurchaseSuccess(object result)
    {
        string packageId = (string)result;

        foreach (var data in DataCenter.Instance.IAPDataDic.Values)
        {
            if (data.PackageId == packageId)
            {
                if (currentStatus == MenuStatus.ColoringPageMenu)
                {
                    ListPrintingMaterial(data.Id);
                }

                AnalyticsLogCenter.LogEvent("purchase_success", new Dictionary<string, object>()
                {
                    { "package_id", data.Id }
                });
                break;
            }
        }
    }

    void OnPurchaseFail(object result)
    {
        string failReason = (string)result;

        AnalyticsLogCenter.LogEvent("purchase_fail", new Dictionary<string, object>()
        {
            { "reason", failReason }
        });

        Debug.Log("Purchase faill: " + failReason);
    }

    public void ShowColoringPage(string packageId)
    {
        PackageData data;

        if (DataCenter.Instance.IAPDataDic.TryGetValue(packageId, out data))
        {
            okButton.SetActive(false);
            Debug.Log("Material List Count: " + DataCenter.Instance.GetPackageMaterialData(data).Count);
            //packageScrollView.UpdateData(DataCenter.Instance.GetPackageMaterialData(data));
            //packageScrollView.ScrollTo(GameSave.LastSelectedColoringPageCellIndex);
        }
    }

    public void ListPrintingMaterial(string packageId, int scrollToIndex = 1)
    {
        PackageData package;
        List<PackageCellData> packageList = new List<PackageCellData>();

        if (DataCenter.Instance.IAPDataDic.TryGetValue(packageId, out package))
        {
            packageList.Add(new PackageCellData(package.Id, Localization.Get(package.PackageName), SpriteDicCenter.Instance.PrintingMaterialSpriteDic.GetSpriteByName(package.PackagePreview),
                Localization.Get(package.PackageName), packageList.Count + 1, PackageCellData.CellType.PackageFrontFaceCell));

            GameSave.LastShownPackage = packageId;
            List<PrintingMaterialData> materialList = DataCenter.Instance.GetPackageMaterialData(package);

            foreach (var material in materialList)
            {
                PackageCellData cell = new PackageCellData(material.Id + "", Localization.Get(material.ColoringPaperName), SpriteDicCenter.Instance.PrintingMaterialSpriteDic.GetSpriteByName(material.Perview),
                    Localization.Get(material.ColoringPaperName), packageList.Count + 1, PackageCellData.CellType.ColoringPageCell);
                cell.materialData = material;
                cell.purchased = DataCenter.Instance.GetPackageStatus(packageId);
                packageList.Add(cell);
                if (packageList.Count == 2)
                {
                    this.SetSelectedImage(material, false);
                }
            }
        }

        packageScrollView.UpdateData(packageList);
        packageScrollView.ScrollTo(scrollToIndex);

        if (selectionList.Count > 0)
        {
            for (int i = 0; i < selectionList.Count; i++)
            {
                Destroy(selectionList[i]);
            }

            selectionList = new List<GameObject>();
        }

        //selectedImage = new List<PrintingMaterialData>();

        int count = 0;

        PackageData data;

        if (DataCenter.Instance.IAPDataDic.TryGetValue(packageId, out data))
        {
            GameSave.LastShownPackage = packageId;

            List<PrintingMaterialData> materialList = DataCenter.Instance.GetPackageMaterialData(data);

            if (materialList != null)
            {
                bool isPurchased = DataCenter.Instance.GetPackageStatus(packageId);

                /*GameObject preview = Instantiate<GameObject>(Resources.Load("Prefabs/Menu/PurchaseButton") as GameObject, PrintingMaterialMenu.content);
                preview.transform.localScale = Vector3.one;

                preview.transform.FindChild("Image").transform.FindChild("Image").GetComponent<Image>().sprite = SpriteDicCenter.Instance.PrintingMaterialSpriteDic.GetSpriteByName(data.PackagePreview);
                preview.GetComponentInChildren<Text>().text = Localization.Get(data.PackageName);

                preview.GetComponent<Button>().onClick.AddListener(Back);

                selectionList.Add(preview);

                for (int i = 0; i < materialList.Count; i++)
                {

                    GameObject go = Instantiate<GameObject>(Resources.Load("Prefabs/Menu/PrintingPaper") as GameObject, PrintingMaterialMenu.content);
                    go.name = "ColoringPaper" + count;
                    go.transform.localScale = Vector3.one;

                    MaterialCell cell = go.transform.FindChild("paper").transform.FindChild("PrintingPaperChild").GetComponent<MaterialCell>();

                    cell.SetData(materialList[i], !isPurchased);

                    selectionList.Add(go);

                    count++;
                }*/

                if (!isPurchased)
                {
                    Product p = Purchaser.Instance.GetLocalizedProduct(data.PackageId);
                    PurchaseButton.GetComponentInChildren<Text>().text = string.Format("{0} " + Localization.Get("txt_action_purchase"),
                        p != null ? p.metadata.isoCurrencyCode + p.metadata.localizedPrice.ToString() : data.Price);
                    Localization.SetFont(PurchaseButton.GetComponentInChildren<Text>());
                    Button button = PurchaseButton.GetComponent<Button>();
                    if (button != null)
                    {
                        string pId = data.PackageId;

                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() =>
                        {
                            Purchase(pId);
                        });
                    }
                }

                PurchaseButton.SetActive(!isPurchased);
                SetFunctionButtonActive(isPurchased);

                PrintingMaterialMenu.content.anchoredPosition = Vector2.zero;
/*
                SelectionButton.isOn = false;
                SetSelectionMode(false);

                if (selectionList.Count > 1 && isPurchased)
                {

                    Toggle t = selectionList[1].GetComponent<Toggle>();
                    t.isOn = true;
                }
*/

                AnalyticsLogCenter.LogEvent("show_package", new Dictionary<string, object>()
                {
                    { "package_id", packageId },
                    { "is_purchased", isPurchased }
                });
            }
        }
    }

    public void ShowPackageList()
    {
        if (packageScrollView == null)
        {
            return;
        }

        SetFunctionButtonActive(false);
        PurchaseButton.SetActive(false);

        List<PackageCellData> packageList = new List<PackageCellData>();
        foreach (var package in DataCenter.Instance.IAPDataDic.Values)
        {
            packageList.Add(new PackageCellData(package.Id, Localization.Get(package.PackageName), SpriteDicCenter.Instance.PrintingMaterialSpriteDic.GetSpriteByName(package.PackagePreview),
                Localization.Get(package.PackageName), packageList.Count + 1, PackageCellData.CellType.PackageCell));
        }

        packageScrollView.UpdateData(packageList);
        packageScrollView.ScrollTo(GameSave.LastSelectedPackageCellIndex, 0.8f);
    }


    public void SetSelectionMode(bool isOn)
    {
        ToggleGroup group = PrintingMaterialMenu.content.GetComponent<ToggleGroup>();

        foreach (var t in PrintingMaterialMenu.content.GetComponentsInChildren<Toggle>())
        {
            t.group = isOn ? null : group;
            if (!isOn)
            {
                t.isOn = false;
            }
        }
    }

    public void DeselectedImage()
    {
        selectedImage = new List<PrintingMaterialData>();
    }

    public void SetSelectedImage(PrintingMaterialData filename, bool enterDrawWhenAlreadySelected)
    {
        if (!selectedImage.Contains(filename))
        {
            selectedImage = new List<PrintingMaterialData>();
            selectedImage.Add(filename);
        }
        else if (enterDrawWhenAlreadySelected && GameSave.ColoringMode == 0)
        {
            DrawButtonClick();
        }

        SetFunctionButtonActive(selectedImage.Count > 0);
    }

    public void Back()
    {
        NativeHelper.GenerateImpactFeedback();

        SoundManager.Instance.Play("ocean_saver_generic_back_button_sfx");

        if (currentStatus == MenuStatus.PrintingPackageMenu)
        {
            SceneTransitHandler.LoadScene("Aquarium");
        }
        else
        {
            SwitchMenu(MenuStatus.PrintingPackageMenu);
        }
    }

    public void QuitPackage(string packageId)
    {
        SwitchMenu(MenuStatus.PrintingPackageMenu);
    }

    public void SelectPackage(string packageId)
    {
        GameSave.SelectedPackageId = packageId;
        SwitchMenu(MenuStatus.ColoringPageMenu);
        ListPrintingMaterial(packageId);
    }

    protected void SwitchMenu(MenuStatus status)
    {
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        if (currentStatus != status)
        {
            currentStatus = status;
            switch (currentStatus)
            {
                case MenuStatus.ColoringPageMenu:
                    //ShowColoringPage(GameSave.SelectedPackageId);
                    break;
                case MenuStatus.PrintingPackageMenu:
                    ShowPackageList();
                    break;
            }
        }
    }

    protected void SwictMenu(MenuStatus status)
    {
/*
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        if (currentStatus != status)
        {

            PrintingMaterialMenu.gameObject.SetActive(status == MenuStatus.PrintingMaterialMenu);
            PrintingPackageMenu.gameObject.SetActive(status == MenuStatus.PrintingPackageMenu);
            currentStatus = status;
        }
  */
    }

    public void SetFunctionButtonActive(bool isActive)
    {
        if (GameSave.ColoringMode == 0)
        {
            SaveButton.SetActive(false);
            PrintButton.SetActive(false);
            drawButton.SetActive(isActive);
            scanButton.SetActive(false);
        }
        else if (GameSave.ColoringMode == 1)
        {
            SaveButton.SetActive(isActive);
            PrintButton.SetActive(isActive);
            drawButton.SetActive(false);
            scanButton.SetActive(isActive);
        }
    }

    public void SaveToCameraRoll()
    {
        NativeHelper.GenerateImpactFeedback();
        AnalyticsLogCenter.LogEvent("color_pages_save_button_clicked");
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        foreach (var image in selectedImage)
        {
            AnalyticsLogCenter.LogEvent("save_paper", new Dictionary<string, object>()
            {
                { "paper_name", image.Material }
            });
        }

        var papers = from a in selectedImage
            select a.Material;
        PermissionHandler.Instance.SaveToCameraRoll(new List<string>(papers));
    }

    public void PrintColoringPaper()
    {
        NativeHelper.GenerateImpactFeedback();
        AnalyticsLogCenter.LogEvent("print_button_clicked");

        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        if (selectedImage.Count > 0)
        {
            foreach (var image in selectedImage)
            {
                AnalyticsLogCenter.LogEvent("tried_to_print_paper", new Dictionary<string, object>()
                {
                    { "paper_name", image.Material }
                });
            }

#if UNITY_ANDROID && !UNITY_EDITOR
			StartCoroutine(PrintInAndroid());
#endif

#if UNITY_IOS && !UNITY_EDITOR
			StringBuilder builder = new StringBuilder();
			foreach(var s in selectedImage){

                builder.Append(PrintingManager.GetPrintingMaterialPath(s.Material, PrintingManager.PrintingMaterialPath)).Append(",");
			}

			PrintingManager.Instance.Print(builder.ToString().TrimEnd(','));
#endif

#if UNITY_EDITOR
            Debug.Log("Cannot print in Editor");
            OnPrintingDone(true);
#endif
        }
        else
        {
            PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_select_at_least_one_paper");
        }
    }

    protected IEnumerator PrintInAndroid()
    {
        if (selectedImage.Count > 1)
        {
            PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_cannot_print_multiple");
        }
        else
        {
            string path = PrintingManager.GetPrintingMaterialPath(selectedImage[0].Material, PrintingManager.PrintingMaterialPath);

            var www = new WWW(path);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(string.Format("Can't read file {0}, {1}", path, www.error));
            }

            PrintingManager.Instance.PrintInAndroid(www.bytes);
        }
    }

    public void Purchase(string id)
    {
        NativeHelper.GenerateImpactFeedback();
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");

        AnalyticsLogCenter.LogEvent("purchase_clicked", new Dictionary<string, object>()
        {
            { "package_id", id }
        });

        Purchaser.Instance.Purchase(id);
#if UNITY_EDITOR
        OnPurchaseSuccess(id);
#endif
    }

    public void DrawButtonClick()
    {
        NativeHelper.GenerateImpactFeedback();
        AnalyticsLogCenter.LogEvent("draw_button_clicked");

        //Crash if put it in Java Callback
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");
        Draw();
    }

    public void Draw()
    {
        if (selectedImage.Count > 1)
        {
            PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_cannot_draw_multiple");
        }
        else if (selectedImage.Count > 0)
        {
            var data = selectedImage[0];
            if (AppData.SelectedPageConfig != null && AppData.SelectedPageConfig.UniqueId == System.IO.Path.GetFileNameWithoutExtension(data.Material))
            {
                SceneTransitHandler.LoadScene("ColoringBook", true);
            }
            else
            {
                StartCoroutine(ReadTextureForDraw());
            }
        }
    }

    IEnumerator ReadTextureForDraw()
    {
        string path = PrintingManager.GetPrintingMaterialPath(selectedImage[0].Material, PrintingManager.DrawingMaterialPath);
        Debug.Log("read texture for draw  " + "file://" + path);
#if UNITY_IOS || UNITY_EDITOR
        var www = new WWW("file://" + path);
#else
        var www = new WWW(path);
#endif
        yield return www;
        // Debug.Log("read texture for draw  " + www.texture);

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(string.Format("Can't read file {0}, {1}", path, www.error));
        }
        else
        {
            SetPageConfigForDraw(www.texture, selectedImage[0]);
        }
    }

    public void SetPageConfigForDraw(Texture2D texture, PrintingMaterialData data)
    {
        string id = System.IO.Path.GetFileNameWithoutExtension(data.Material);

        if (AppData.SelectedPageConfig == null || AppData.SelectedPageConfig.UniqueId != id)
        {
            AdvanceColoringPageConfig pageConfig = ScriptableObject.CreateInstance<AdvanceColoringPageConfig>();
            pageConfig.UniqueId = id;

            //Check device ram

            int memorySize = SystemInfo.systemMemorySize;

            if (memorySize <= 1024)
            {
                Debug.Log("Reduce texture dimension due to the memory size - " + memorySize);
                ImageDataWriter.Scale(texture, 1024, 1024);
            }

            pageConfig.SetColoringTexture(texture, null);

            //Will not save progress if without name
            pageConfig.name = "ColoringPaper";

            AppData.SelectedPageConfig = pageConfig;
            AppData.SelectedPageModel = data.Model;
        }

        //Clear memory
        TextureCache.Instance.ClearCache();
        SpriteDicCenter.Instance.ReleaseMemory();
        GL.Clear(true, true, Color.black);

        SceneTransitHandler.LoadScene("ColoringBook", true);
    }

    public void OnPrintingDone(bool success)
    {
#if UNITY_IOS
        if (!GameSave.HasSuccessfullyPrintedColoringPaper && !GameSave.HasSuccessfullyScannedColoringPaper)
        {
            if (success)
            {
                AnalyticsLogCenter.LogEvent("has_printed_successful");
                GameSave.HasSuccessfullyPrintedColoringPaper = true;
            }
            //         else
            //         {
            //	if(!GameSave.IsCollectedPrintingProblemReason){

            //		Debug.Log("Show PrintingProblemPopup");
            //		PopupManager.Instance.QuickShowPopup(PopupPrefab.PrintingProblemPopup);
            //	}
            //}
        }
#endif
    }

    public void ScanButtonClick()
    {
        NativeHelper.GenerateImpactFeedback();
        AnalyticsLogCenter.LogEvent("scan_button_clicked");

        //Crash if put it in Java Callback
        SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");
        EnterARCapture();
    }

    public void EnterARCapture()
    {
        ButtonTool.PausingInteractable(scanButton, 1f);
        //ButtonTool.PausingInteractable(scanOKBtn, 1f);

        AnalyticsLogCenter.LogEvent("enter_ar_capture");

        if (!GameSave.HasShownCameraPermission)
        {
            GameSave.HasShownCameraPermission = true;

#if UNITY_ANDROID
			string requestText = Localization.Get("txt_permission_camera") + "\n" + Localization.Get("txt_permission_write_storage_for_sketches");

			PopupManager.Instance.ShowMessagePopup("txt_permissions_title", requestText, ()=>{

				bool hasCameraPermission = AndroidPermissionsManager.IsPermissionGranted(GameSetting.CameraPermission);
				bool hasWriteStoragePermission = AndroidPermissionsManager.IsPermissionGranted(GameSetting.WriteStoragePermission);


				if(!hasCameraPermission || !hasWriteStoragePermission){

					List<string> requestPermissions = new List<string>();

					if(!hasCameraPermission) requestPermissions.Add(GameSetting.CameraPermission);
					if(!hasWriteStoragePermission) requestPermissions.Add(GameSetting.WriteStoragePermission);

                    IEnumerator checker = AndroidPermissionChecker(requestPermissions, 0.3f, EnterARCapture);
                    StartCoroutine(checker);

                    AndroidPermissionsManager.RequestPermission (requestPermissions.ToArray(), null, (permission)=>{

                        StopCoroutine(checker);
                    });
				}
			});
#else
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_camera", EnterARCapture, true);
#endif
        }
        else
        {
#if UNITY_IOS && !UNITY_EDITOR
            if(NativeHelper.GetCameraPermission() == PermissionStatus.Undetermined)
            {
                NativeHelper.onCameraPermissionUpdated += (permission) =>
                {
                    NativeHelper.onCameraPermissionUpdated = null;
                    EnterARCapture();
                };
                NativeHelper.RequestCameraPermission();
                return;
            }
            else if(NativeHelper.GetCameraPermission() == PermissionStatus.Denied)
            {
                PopupManager.Instance.ShowPermissionPopup("txt_permissions_title", "txt_permission_camera", ()=>
                {
                    NativeHelper.OpenSetting();
                }, ()=>
                {

                }, true);
                return;
            }
#elif UNITY_ANDROID
			bool hasCameraPermission = AndroidPermissionsManager.IsPermissionGranted(GameSetting.CameraPermission);
			bool hasWriteStoragePermission = AndroidPermissionsManager.IsPermissionGranted(GameSetting.WriteStoragePermission);

			if(!hasCameraPermission || !hasWriteStoragePermission){

				List<string> requestPermissions = new List<string>();

				if(!hasCameraPermission) requestPermissions.Add(GameSetting.CameraPermission);
				if(!hasWriteStoragePermission) requestPermissions.Add(GameSetting.WriteStoragePermission);

                string requestText = (hasCameraPermission? "" : Localization.Get("txt_permission_camera"))
				+ "\n"
                + (hasWriteStoragePermission? "": Localization.Get("txt_permission_write_storage_for_sketches"));


				PopupManager.Instance.ShowPermissionPopup("txt_permissions_title", requestText, ()=>{

                    IEnumerator checker = AndroidPermissionChecker(requestPermissions, 0.3f, EnterARCapture);
                    StartCoroutine(checker);

                    AndroidPermissionsManager.RequestPermission(requestPermissions.ToArray(), null, (permission) => {

                        StopCoroutine(checker);
                    });
				});

				return;
			}
#endif
            // VuforiaIniter.Instance.InitVuforia(((success) =>
            // {
            //     LoadScene("PreviewScene");
            // }));
        }
    }

    IEnumerator AndroidPermissionChecker(List<string> permissions, float interval, PermissionCallback callback)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            foreach (string p in permissions)
            {
                if (!AndroidPermissionsManager.IsPermissionGranted(p))
                {
                    continue;
                }
            }

            break;
        }

        if (callback != null)
        {
            callback();
        }
    }

    public void LoadScene(string target)
    {
        SceneTransitHandler.LoadScene(target);
    }
}