using ProGamer;

namespace UnityEngine.UI.Extensions
{
    public class PackageScrollViewCell
        : FancyScrollViewCell<PackageCellData, PackageScrollViewContext>
    {
        [SerializeField] Animator animator;
        [SerializeField] Text message;
        [SerializeField] Image image;
        [SerializeField] Button cellButton;
        [SerializeField] Image cellTitleBG;
        [SerializeField] Button okButton;
        [SerializeField] Text okButtonText;

        readonly int scrollTriggerHash = Animator.StringToHash("scroll");
        PackageScrollViewContext context;

        public PackageCellData Data;
        public bool IsPackage = true;
        public bool IsColoringPage = false;
        public bool IsImageBeingSet = false;
        protected string cellImagePath = "";

        public System.DateTime startTime;

        void Start()
        {
            startTime = System.DateTime.UtcNow;

            var rectTransform = transform as RectTransform;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            UpdatePosition(0);

            if (cellButton != null)
            {
                cellButton.onClick.AddListener(OnPressedCell);
            }

            if (okButton != null)
            {
                okButton.onClick.AddListener(OnPressedOK);
                okButton.gameObject.SetActive(false);
            }

            // pausing interactable a bit to avoid tutorial bug
            ButtonTool.PausingInteractable(cellButton, 1f);
            ButtonTool.PausingInteractable(okButton, 1f);
        }

        /// <summary>
        /// コンテキストを設定します
        /// </summary>
        /// <param name="context"></param>
        public override void SetContext(PackageScrollViewContext context)
        {
            this.context = context;
        }

        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            if (!visible)
            {
                IsImageBeingSet = false;
            }
        }

        /// <summary>
        /// セルの内容を更新します
        /// </summary>
        /// <param name="itemData"></param>
        public override void UpdateContent(PackageCellData d)
        {
            if ((this.Data != null && d == null) ||
                (this.Data == null && d != null) ||
                (this.Data != null && d != null && this.Data.Id != d.Id))
            {
                IsImageBeingSet = false;
            }

            this.Data = d;

            if (d != null && d.Id != "")
            {
                Sprite preview = null;

                if (!IsImageBeingSet)
                {
                    if (d.sprite != null)
                    {
                        this.image.sprite = d.sprite;

                        Vector2 buttonSize = image.rectTransform.sizeDelta;
                        this.image.rectTransform.sizeDelta = new Vector2(buttonSize.y / Screen.height * Screen.width, buttonSize.y);
                    }

                    IsImageBeingSet = true;

                    // pausing interactable a bit to avoid tutorial bug
                    ButtonTool.PausingInteractable(cellButton, 0.5f);
                    ButtonTool.PausingInteractable(okButton, 0.5f);
                }

                if (okButtonText != null)
                {
                    okButtonText.SetLocalization("");
                }

                message.text = d.Name;
                Localization.SetFont(message);


                if (cellTitleBG != null && !cellTitleBG.gameObject.activeSelf)
                {
                    cellTitleBG.gameObject.SetActive(true);
                }
            }

            if (context != null)
            {
                var isSelected = context.SelectedIndex == DataIndex;
                if (isSelected)
                {
                    if (this.Data.cellType == PackageCellData.CellType.ColoringPageCell && this.Data.purchased)
                    {
                        PrintingUIManager.Instance.SetSelectedImage(this.Data.materialData, false);
                        PrintingUIManager.Instance.SetFunctionButtonActive(true);
                    }
                    else if (this.Data.cellType == PackageCellData.CellType.PackageFrontFaceCell)
                    {
                        PrintingUIManager.Instance.SetFunctionButtonActive(false);
                        PrintingUIManager.Instance.DeselectedImage();
                    }
                }
            }
        }

        /// <summary>
        /// セルの位置を更新します
        /// </summary>
        /// <param name="position"></param>
        public override void UpdatePosition(float position)
        {
            if (!animator)
                return;
            animator.Play(scrollTriggerHash, -1, position);
            animator.speed = 0;
        }

        public void OnPressedCell()
        {
            NativeHelper.GenerateImpactFeedback();
            ButtonTool.PausingInteractable(cellButton, 0.3f);
            ButtonTool.PausingInteractable(okButton, 0.3f);
            if (context != null)
            {
                context.OnPressedCell(this);
            }
        }

        public void OnPressedOK()
        {
            NativeHelper.GenerateImpactFeedback();
            ButtonTool.PausingInteractable(cellButton, 0.3f);
            ButtonTool.PausingInteractable(okButton, 0.3f);
            if (this.Data.cellType == PackageCellData.CellType.PackageCell)
            {
                PrintingUIManager.Instance.SelectPackage(this.Data.Id);
            }
        }
    }
}