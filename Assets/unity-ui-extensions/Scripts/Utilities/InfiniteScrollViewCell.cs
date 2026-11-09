using ProGamer;
using UnityEngine.Assertions;

namespace UnityEngine.UI.Extensions
{
    public class InfiniteScrollViewCell
        : FancyScrollViewCell<AquariumData, InfiniteScrollViewContext>
    {
        [SerializeField] Animator animator;
        [SerializeField] Image image;
        [SerializeField] Button cellButton;
        [SerializeField] Image cellTitleBG;
        [SerializeField] Button okButton;
        [SerializeField] Text okButtonText;
        [SerializeField] GameObject hatcheryButtonBg;
        [SerializeField] GameObject metaverseButtonBg;
        [SerializeField] Button addButton;

        readonly int scrollTriggerHash = Animator.StringToHash("scroll");
        InfiniteScrollViewContext context;

        private AquariumData data;
        private bool isImageSet;

        private void Start()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            UpdatePosition(0);
            cellButton.onClick.AddListener(OnPressedCell);
            okButton.onClick.AddListener(OnPressedOK);
            okButton.gameObject.SetActive(false);
            addButton.onClick.AddListener(OnPressedPurchase);

            // pausing interactable a bit to avoid tutorial bug
            ButtonTool.PausingInteractable(cellButton, 1f);
            ButtonTool.PausingInteractable(okButton, 1f);
        }

        void OnEnable()
        {
            SetupCellButton();
            EventManager.StartListening(EventList.OnPurchaseSuccess, OnPurchaseSuccess);
        }

        void OnDisable()
        {
            EventManager.StopListening(EventList.OnPurchaseSuccess, OnPurchaseSuccess);
        }

        /// <summary>
        /// コンテキストを設定します
        /// </summary>
        /// <param name="context"></param>
        public override void SetContext(InfiniteScrollViewContext context)
        {
            this.context = context;
        }

        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            if (!visible) isImageSet = false;
        }

        /// <summary>
        /// セルの内容を更新します
        /// </summary>
        public override void UpdateContent(AquariumData d)
        {
            Assert.IsNotNull(d);
            if (context != null)
            {
                var isSelected = context.SelectedIndex == DataIndex;
                if (okButton != null && okButton.gameObject.activeSelf != isSelected)
                {
                    okButton.gameObject.SetActive(isSelected);
                }
            }

            if (data == d) return;
            if (data == null || (data != null && data.AquariumId != d.AquariumId))
            {
                isImageSet = false;
            }

            data = d;
            Sprite preview = null;

            if (!isImageSet)
            {
                if (!string.IsNullOrEmpty(d.SnapShotPath))
                {
                    preview = ImageDataWriter.LoadSprite(d.SnapShotPath);
                }

                if (preview != null)
                {
                    image.sprite = preview;
                    Vector2 buttonSize = image.rectTransform.sizeDelta;
                    image.rectTransform.sizeDelta = new Vector2(buttonSize.y / Screen.height * Screen.width, buttonSize.y);
                }
                else
                {
                    image.sprite = SpriteDicCenter.Instance.ThemeSprite.GetSpriteByName(d.Data.Icon);
                }

                isImageSet = true;
                // pausing interactable a bit to avoid tutorial bug
                ButtonTool.PausingInteractable(cellButton, 1f);
                ButtonTool.PausingInteractable(okButton, 1f);
            }


            hatcheryButtonBg.SetActive(d.Index == 0);
            metaverseButtonBg.SetActive(d.Index != 0);
            okButtonText.SetLocalization(d.Index == 0 ? "txt_hatchery" : "txt_metaverse");
            cellTitleBG.gameObject.SetActive(true);
            if (d.Index == 0) SetupHatcheryButton();
            else SetupMetaverseButton();
        }

        /// <summary>
        /// セルの位置を更新します
        /// </summary>
        /// <param name="position"></param>
        public override void UpdatePosition(float position)
        {
            animator.Play(scrollTriggerHash, -1, position);
            animator.speed = 0;
        }

        private void OnPressedCell()
        {
            NativeHelper.GenerateImpactFeedback();
            ButtonTool.PausingInteractable(cellButton, 0.3f);
            ButtonTool.PausingInteractable(okButton, 0.3f);
            if (okButton.gameObject.activeSelf) OnPressedOK();
            if (context != null) context.OnPressedCell(this); // scroll
        }

        private void OnPressedOK()
        {
            NativeHelper.GenerateImpactFeedback();
            GameSave.SelectedAquarium = data.AquariumId;
            ButtonTool.PausingInteractable(cellButton, 0.3f);
            ButtonTool.PausingInteractable(okButton, 0.3f);
            if (data.Index == 0) OnPressHatcheryButton();
            else OnPressMetaverseButton();
        }

        private void OnPressedPurchase()
        {
            PopupManager.Instance.QuickShowPopup(PopupPrefab.PurchasePopup, true);
            var popup = PopupManager.Instance.ActivePopup as PurchasePopup;
            popup.ShowBandItems();
            popup.PurchaseSuccessCallback = () =>
            {
                ImageDataManager.Instance.FillImageDataLists(true);
            };
        }

        #region Hatchery Functions

        private void SetupHatcheryButton()
        {
            addButton.gameObject.SetActive(false);
        }

        private void OnPressHatcheryButton()
        {
            SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");
            AquariumManageUI.Instance.ShowARAquarium();
        }

        #endregion

        #region Metaverse Functions

        private void SetupMetaverseButton()
        {
            addButton.gameObject.SetActive(DataCenter.Instance.GetBandPurchaseStatus() != 3 || !DataCenter.Instance.IsAccessoryPurchased());
        }

        private void OnPressMetaverseButton()
        {
            if (!ServerCenter.Instance.IsLoggedIn && !Application.isEditor)
            {
                PopupManager.Instance.QuickShowPopup(PopupPrefab.SignInWithApplePopup, true);
            }
            else
            {
                SoundManager.Instance.Play("ocean_saver_generic_ui_sfx");
                AquariumManageUI.Instance.ShowARAquarium();
            }
        }

        #endregion

        private void OnPurchaseSuccess(object result)
        {
            SetupCellButton();
        }

        private void SetupCellButton()
        {
            if (data == null) return;
            if (data.Index == 0) SetupHatcheryButton();
            else SetupMetaverseButton();
        }
    }
}