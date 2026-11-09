using UnityEngine;
using UnityEngine.UI;

namespace ProGamer
{
    [RequireComponent(typeof(Text))]
    public class UILocalize : Localize
    {
        [SerializeField] protected Text textField;

        public Text TextField
        {
            get
            {
                if (textField == null) textField = GetComponent<Text>();
                return textField;
            }
        }

        protected override void TextSetter()
        {
            if (TextField) Localization.SetText(key, textField);
        }

        protected override void TextSetterWithParameters()
        {
            if (TextField) Localization.SetTextWithParameters(key, ref textField, parameters);
        }

        protected override void TextSetterWithFormat()
        {
            if (TextField) Localization.SetTextWithFormat(key, ref textField, format);
        }
    }
}