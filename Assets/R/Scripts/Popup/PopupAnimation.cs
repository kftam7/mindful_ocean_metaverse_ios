using UnityEngine;
using System.Collections;

namespace ProGamer
{
    public delegate void SimplePopupCallback();

    public abstract class PopupAnimation : MonoBehaviour
    {
        public virtual void Show()
        {
            Show(null);
        }

        public abstract void Show(SimplePopupCallback callback);

        public virtual void Close()
        {
            Close(null);
        }

        public abstract void Close(SimplePopupCallback callback);
    }
}