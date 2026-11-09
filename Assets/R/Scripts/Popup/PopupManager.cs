using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum PopupProperty
{
    Prefab,
    Title,
    Description,
    ConfirmButton,
    DismissButton,
    CrossButton,
    TimeOut,
    FrameSize,
    Image,
    None
}

public delegate bool ConditionResult();

public enum PopupPrefab
{
    SimplePopup,
    SimpleBadPopup,
    ConfirmPopup,
    MathGatePopup,
    LoadingPopup,
    SettingPopup,
    QuizPopup,
    HowToPlayPopup,
    TaskPopup,
    PausePopup,
    DeathPopup,
    ARPopup,
    SharePopup,
    ResultsPopup,
    SimpleWebviewPopup,
    FullWebviewPopup,
    UnlockPopup,
    PrintingProblemPopup,

    EventInfoPopup,
    ModelPreviewPopup,
    PurchasePopup,
    SubmitPopup,
    DisclaimerPopup,
    SubmitDisclaimerPopup,
    SubmitConfirmPopup,
    SubmitResultPopup,
    ReplaceFishListPopup,
    SubmitCooldownPopup,
    QrCodePopup,
    SignInWithApplePopup,
    SignInWithAppleWhyPopup,
    HintPopup,
    GestureHintPopup,
    ProcessingPopup,
    NewVersionPopup,
    FishHintPopup,
    TutorialMessagePopup,
    TutorialMainFlowPopup,
    
    SelectDomainPopup
}

namespace ProGamer
{
    [DisallowMultipleComponent]
    public class PopupManager : Singleton<PopupManager>
    {
        protected Stack<SimplePopup> popupList = new Stack<SimplePopup>();
        protected Queue<Hashtable> queuePopupData = new Queue<Hashtable>();

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            popupList = new Stack<SimplePopup>();
            queuePopupData = new Queue<Hashtable>();
        }

        public SimplePopup ActivePopup
        {
            get
            {
                if (popupList.Count > 0) return popupList.Peek();
                else return null;
            }
            set { popupList.Push(value); }
        }

        public Hashtable QueuePopupData
        {
            get
            {
                if (queuePopupData.Count > 0) return queuePopupData.Dequeue();
                else return null;
            }
        }

        //Call by popup, remove object in popup list
        public void RemoveActivePopup()
        {
            if (popupList.Count > 0)
            {
                popupList.Pop();
            }
        }

        public GameObject GetPopupPrefab(PopupPrefab popup)
        {
            return Resources.Load("Prefabs/Popup/" + popup.ToString()) as GameObject;
        }

        public SimplePopup ShowPopup(Hashtable parameters, bool isTop = false)
        {
            Transform parent;
            if (isTop)
            {
                parent = TopPopupContainer.Instance != null ? TopPopupContainer.Instance.transform : transform;
            }
            else
            {
                parent = PopupContainer.Instance != null ? PopupContainer.Instance.transform : transform;
            }

            return ShowPopup(parameters, parent);
        }

        public SimplePopup ShowPopup(Hashtable parameters, Transform parent)
        {
            GameObject popup = ReuseableObjectManager.Instantiate((GameObject)parameters[PopupProperty.Prefab], Vector3.zero, Quaternion.identity);
            popup.transform.SetParent(parent, false);
            popup.transform.localScale = Vector3.one;

            //Set to the top in hierarchy 
            popup.transform.SetAsLastSibling();

            popupList.Push(popup.GetComponent<SimplePopup>());
            ActivePopup.SetParameters(parameters);

            ActivePopup.Show();

            return ActivePopup;
        }

        public void AddPopupToQueueAndShow(Hashtable parameters, bool isTop = false)
        {
            Transform parent;
            if (isTop)
            {
                parent = TopPopupContainer.Instance != null ? TopPopupContainer.Instance.transform : transform;
            }
            else
            {
                parent = PopupContainer.Instance != null ? PopupContainer.Instance.transform : transform;
            }

            AddPopupToQueueAndShow(parameters, parent);
        }

        public void AddPopupToQueueAndShow(Hashtable parameters, Transform parent)
        {
            GameObject popup = ReuseableObjectManager.Instantiate((GameObject)parameters[PopupProperty.Prefab], Vector3.zero, new Quaternion(0, 0, 0, 0));

            popup.transform.SetParent(parent, false);
            popup.transform.localScale = Vector3.one;

            //Set to the top in hierarchy 
            popup.transform.SetAsLastSibling();

            popup.SetActive(false);

            SimplePopup simplePopup = popup.GetComponent<SimplePopup>();
            queuePopupData.Enqueue(parameters);

            QueueSystem.AddQueueWithExecution(simplePopup);
        }


        #region General Popup

        public void QuickShowPopup(PopupPrefab prefab, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(prefab);

            ShowPopup(hashtable, isTop);
        }

        public void QuickShowPopupWebview(string url, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimpleWebviewPopup);
            hashtable["URL"] = url;

            ShowPopup(hashtable, isTop);
        }

        public void QuickShowFullPopupWebview(string url, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.FullWebviewPopup);
            hashtable["URL"] = url;

            ShowPopup(hashtable, isTop);
        }

        public void QuickShowQueuePopup(PopupPrefab prefab, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(prefab);

            AddPopupToQueueAndShow(hashtable, isTop);
        }

        public void ShowMessagePopup(string title, string desc, Action confirmCallback = null, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimplePopup);
            hashtable[PopupProperty.Title] = title;
            hashtable[PopupProperty.Description] = desc;
            hashtable[PopupProperty.ConfirmButton] = new ButtonFormat("txt_ok", confirmCallback);
            ShowPopup(hashtable, isTop);
        }
        
        public void ShowBadMessagePopup(string desc, Action confirmCallback = null, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimpleBadPopup);
            hashtable[PopupProperty.Description] = desc;
            hashtable[PopupProperty.ConfirmButton] = new ButtonFormat("txt_ok", confirmCallback);
            ShowPopup(hashtable, isTop);
        }

        public void ShowConfirmPopup(string title, string desc, Action confirmCallback = null, Action cancelCallback = null, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable
            {
                [PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.ConfirmPopup),
                [PopupProperty.Title] = title,
                [PopupProperty.Description] = desc,
                [PopupProperty.ConfirmButton] = new ButtonFormat("txt_ok", confirmCallback),
                [PopupProperty.DismissButton] = new ButtonFormat("txt_cancel", cancelCallback)
            };
            ShowPopup(hashtable, isTop);
        }

        public void ShowPermissionPopup(string title, string desc, Action confirmCallback = null, Action cancelCallback = null, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimplePopup);
            hashtable[PopupProperty.Title] = title;
            hashtable[PopupProperty.Description] = desc;
            hashtable[PopupProperty.ConfirmButton] = new ButtonFormat("txt_ok", confirmCallback);
            hashtable[PopupProperty.DismissButton] = new ButtonFormat("txt_cancel", cancelCallback);
            ShowPopup(hashtable, isTop);
        }

        public void NoInternetPopup(bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimplePopup);
            hashtable[PopupProperty.Title] = "txt_network_problem";
            hashtable[PopupProperty.Description] = "txt_connection_fail";
            hashtable[PopupProperty.ConfirmButton] = new ButtonFormat("txt_ok", null, SpriteAlignment.BottomCenter, false, new Vector3(0, 100, 0));
            ShowPopup(hashtable, isTop);
        }

        public void UnstableInternetPopup(bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimplePopup);
            hashtable[PopupProperty.Title] = "txt_network_problem";
            hashtable[PopupProperty.Description] = "txt_unstable_internet";
            hashtable[PopupProperty.ConfirmButton] = new ButtonFormat("txt_ok", null, SpriteAlignment.BottomCenter, false, new Vector3(0, 100, 0));
            ShowPopup(hashtable, isTop);
        }

        public void QuitGamePopup()
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.SimplePopup);
            hashtable[PopupProperty.Title] = "txt_quit_game";
            hashtable[PopupProperty.Description] = "txt_quit_game_content";
            hashtable[PopupProperty.ConfirmButton] = new ButtonFormat("txt_yes", () =>
            {
                Application.Quit();
            }, SpriteAlignment.BottomLeft, false, new Vector3(150, 100, 0));

            hashtable[PopupProperty.DismissButton] = new ButtonFormat("txt_no", null, SpriteAlignment.BottomRight, false, new Vector3(-150, 100, 0));

            AddPopupToQueueAndShow(hashtable);
        }

        public SimplePopup ShowLoadingPopup(bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable[PopupProperty.Prefab] = GetPopupPrefab(PopupPrefab.LoadingPopup);
            return ShowPopup(hashtable, isTop);
        }

        /// <summary>
        /// Shows the advance loading popup with timeout and callback
        /// </summary>
        /// <param name="condition">method for check if loaded, True: Success</param>
        /// <param name="timeoutCallback">execute if timeout</param>
        /// <param name="timeout">Timeout.</param>
        public SimplePopup ShowAdvanceLoadingPopup(ConditionResult condition, SimplePopupCallback timeoutCallback, float timeout)
        {
            SimplePopup popup = ShowLoadingPopup();
            StartCoroutine(LoadingPopupListener(condition, timeoutCallback, timeout, 0.2f, popup));
            return popup;
        }

        /// <summary>
        /// Shows the advance loading popup with timeout and callback
        /// </summary>
        /// <param name="condition">method for check if loaded, True: Success</param>
        /// <param name="timeout">Timeout.</param>
        public void ShowAdvanceLoadingPopup(ConditionResult condition, float timeout)
        {
            ShowAdvanceLoadingPopup(condition, null, timeout);
        }

        public void ShowMathGatePopup(UnityEngine.Events.UnityAction action, bool isTop = false)
        {
            Hashtable hashtable = new Hashtable();
            hashtable["callback"] = action;
            hashtable[PopupProperty.Prefab] = PopupManager.Instance.GetPopupPrefab(PopupPrefab.MathGatePopup);
            ShowPopup(hashtable, isTop);
        }


        protected IEnumerator LoadingPopupListener(ConditionResult condition, SimplePopupCallback timeoutCallback, float timeout, float checkPeriod, SimplePopup targetPopup)
        {
            float time = 0;
            bool isTimeout = true;

            while (timeout > time)
            {
                yield return new WaitForSeconds(checkPeriod);
                time += checkPeriod;

                if (condition())
                {
                    isTimeout = false;
                    break;
                }
            }

            targetPopup.Close();

            if (isTimeout && timeoutCallback != null)
            {
                timeoutCallback();
            }
        }

        #endregion
    }
}