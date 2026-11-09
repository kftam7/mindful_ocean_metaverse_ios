using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ProGamer
{
    /// <summary>
    /// General reusable popup.
    /// Set Popup size
    /// Set Text
    /// Three buttons: Confirm, Dismiss, Cross
    /// </summary>
    public class GeneralReusablePopup : SimplePopup
    {
        [SerializeField] protected Text titleText;
        [SerializeField] protected Text descriptionText;
        [SerializeField] protected Button confirmButton;
        [SerializeField] protected Button dismissButton;
        [SerializeField] protected Button crossButton;

        //Restore gameobject for reuse
        protected virtual void Restore()
        {
            if (confirmButton)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(Close);
            }

            if (dismissButton)
            {
                dismissButton.onClick.RemoveAllListeners();
                dismissButton.onClick.AddListener(Close);
            }

            if (crossButton)
            {
                crossButton.onClick.RemoveAllListeners();
                crossButton.onClick.AddListener(Close);
            }
        }

        protected override void InitPopup()
        {
            base.InitPopup();

            Restore();

            if (titleText)
            {
                if (parameters.ContainsKey(PopupProperty.Title))
                {
                    titleText.text = Localization.Get((string)parameters[PopupProperty.Title]);
                    titleText.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    titleText.transform.parent.gameObject.SetActive(false);
                }

                Localization.SetFont(titleText);
            }

            if (descriptionText)
            {
                if (parameters.ContainsKey(PopupProperty.Description))
                {
                    descriptionText.text = Localization.Get((string)parameters[PopupProperty.Description]);
                }
                else
                {
                    descriptionText.gameObject.SetActive(false);
                }

                Localization.SetFont(descriptionText);
            }

            SetButton(confirmButton, PopupProperty.ConfirmButton);
            SetButton(dismissButton, PopupProperty.DismissButton);
            SetButton(crossButton, PopupProperty.CrossButton);
        }

        protected virtual void SetButton(Button button, PopupProperty key)
        {
            if (button)
            {
                if (parameters.ContainsKey(key))
                {
                    button.gameObject.SetActive(true);
                    ButtonFormat buttonFormat = (ButtonFormat)parameters[key];

                    if (!string.IsNullOrEmpty(buttonFormat.ButtonText))
                    {
                        Localize l = button.GetComponentInChildren<Localize>();
                        if (l != null)
                        {
                            l.Key = buttonFormat.ButtonText;
                        }
                        else
                        {
                            Debug.Log($"[{GetType().Name}] Cannot found localize component");
                        }
                    }

                    if (buttonFormat.ButtonAction != null) button.onClick.AddListener(buttonFormat.ButtonAction.Invoke);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}