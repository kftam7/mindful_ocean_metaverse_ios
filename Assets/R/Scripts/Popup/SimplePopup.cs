using UnityEngine;
using System.Collections;

namespace ProGamer
{
    /// <summary>
    /// Only for show and close
    /// If popup contains another popup, first call close, then show the new one
    /// </summary>
    [RequireComponent(typeof(PopupAnimation))]
    public class SimplePopup : QueueObject
    {
        protected PopupAnimation transition;

        public PopupAnimation PopupAnimation
        {
            get
            {
                if (transition) return transition;
                return GetComponent<PopupAnimation>();
            }
        }

        public override void QueueStart()
        {
            status = QueueStatus.Executing;
            gameObject.SetActive(true);

            //Set to the top in hierarchy 
            transform.SetAsLastSibling();

            SetParameters(PopupManager.Instance.QueuePopupData);
            PopupManager.Instance.ActivePopup = this;
            Show();
        }

        public override void QueueCompleted()
        {
            status = QueueStatus.Completed;
        }

        protected virtual void OnDisable()
        {
            if (status != QueueStatus.Completed)
            {
                QueueCompleted();
            }
        }


        protected Hashtable parameters;

        /// <summary>
        /// Init Popup, Use by ReusablePopup
        /// </summary>
        /// <param name="parameters">Parameters.</param>
        public virtual void SetParameters(Hashtable parameters)
        {
            this.parameters = parameters;
        }

        protected virtual void InitPopup() { }

        public virtual void Show()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (parameters != null) InitPopup();
            if (PopupWillShowEvent != null) PopupWillShowEvent();
            PopupAnimation.Show(() =>
            {
                PopupDidShow();
                if (PopupDidShowEvent != null)
                    PopupDidShowEvent();
            });
        }

        public virtual void Close()
        {
            PopupWillCloseEvent?.Invoke();
            PopupManager.Instance.RemoveActivePopup();
            PopupAnimation.Close(() =>
            {
                PopupDidClose();
                PopupDidCloseEvent?.Invoke();
            });

            SoundManager.Instance.Play("ocean_saver_generic_back_button_sfx");
        }

        public virtual void CloseQuietly()
        {
            PopupWillCloseEvent?.Invoke();
            PopupManager.Instance.RemoveActivePopup();
            PopupAnimation.Close(() =>
            {
                PopupDidClose();
                PopupDidCloseEvent?.Invoke();
            });
        }

        public delegate void PopupEvent();

        public event PopupEvent PopupWillShowEvent;
        public event PopupEvent PopupDidShowEvent;
        public event PopupEvent PopupWillCloseEvent;
        public event PopupEvent PopupDidCloseEvent;

        public virtual void PopupDidShow() { }

        public virtual void PopupDidClose()
        {
            gameObject.SetActive(false);
        }

        public virtual void BackAction()
        {
            Close();
        }
    }
}