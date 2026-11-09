using UnityEngine;

namespace ProGamer
{
    [DisallowMultipleComponent]
    public abstract class Localize : MonoBehaviour
    {
        [SerializeField] protected string key;

        protected bool mStarted = false;
        protected bool withParameters = false;
        protected object[] parameters;
        protected string format;

        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                OnLocalize();
            }
        }

        protected virtual void OnEnable()
        {
            Localization.instance.LocalizeUpdate += OnLocalize;

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (mStarted) OnLocalize();
        }

        protected virtual void OnDisable()
        {
            Localization.instance.LocalizeUpdate -= OnLocalize;
        }

        protected virtual void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            mStarted = true;
            OnLocalize();
        }

        public virtual void OnLocalize()
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (withParameters)
                {
                    TextSetterWithParameters();
                }
                else if (!string.IsNullOrEmpty(format))
                {
                    TextSetterWithFormat();
                }
                else
                {
                    TextSetter();
                }
            }
        }

        public void Set(string key)
        {
            withParameters = false;
            this.parameters = null;
            this.format = string.Empty;
            this.Key = key;
        }

        public void Set(string key, object[] parameter)
        {
            withParameters = true;
            this.parameters = parameter;
            this.format = string.Empty;
            this.Key = key;
        }

        public void Set(string key, string format)
        {
            withParameters = false;
            this.parameters = null;
            this.Key = key;
            this.format = format;
        }

        protected abstract void TextSetter();
        protected abstract void TextSetterWithParameters();
        protected abstract void TextSetterWithFormat();
    }
}