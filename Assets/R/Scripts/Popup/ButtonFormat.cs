using UnityEngine;
using System.Collections;

namespace ProGamer
{
    public class ButtonFormat : AlignmentObject
    {
        public string ButtonText;
        public System.Action ButtonAction;

        public ButtonFormat()
        {
            SetParameters("", null, SpriteAlignment.Custom, Vector3.zero, false);
        }

        public ButtonFormat(string text)
        {
            SetParameters(text, null, SpriteAlignment.Custom, Vector3.zero, false);
        }

        public ButtonFormat(System.Action action)
        {
            SetParameters("", action, SpriteAlignment.Custom, Vector3.zero, false);
        }

        public ButtonFormat(string text, System.Action action)
        {
            SetParameters(text, action, SpriteAlignment.Custom, Vector3.zero, false);
        }

        public ButtonFormat(string text, System.Action action, SpriteAlignment align, bool isAutoPivot)
        {
            SetParameters(text, action, align, Vector3.zero, isAutoPivot);
        }

        public ButtonFormat(string text, System.Action action, SpriteAlignment align, bool isAutoPivot, Vector3 alignOffset)
        {
            SetParameters(text, action, align, alignOffset, isAutoPivot);
        }

        protected virtual void SetParameters(string text, System.Action action, SpriteAlignment align, Vector3 alignOffset, bool isAutoPivot)
        {
            ButtonText = text;
            ButtonAction = action;
            AlignmentMethod = align;
            IsAutoPivot = isAutoPivot;
            AlignOffset = alignOffset;
        }
    }
}