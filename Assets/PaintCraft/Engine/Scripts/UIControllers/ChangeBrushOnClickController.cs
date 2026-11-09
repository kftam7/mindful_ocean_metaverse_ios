using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PaintCraft.Tools;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace PatinCraft.UI{

    [System.Serializable]
    public class BrushChangedEvent : UnityEvent<string> { };

    public class ChangeBrushOnClickController : MonoBehaviour, IPointerClickHandler {
        public LineConfig LineConfig;
        public Brush Brush;
        public static BrushChangedEvent OnBrushChanged = new BrushChangedEvent();

        void Awake(){
            if (LineConfig == null){
                Debug.LogError("LineConfig must be provided", gameObject);
            }

            if (Brush == null){
                Debug.LogError("Brush could not be null here", gameObject);
            }

            Toggle t = GetComponent<Toggle>();
            if (t != null)
            {
                string usedBrushName = "Bucket Tool";//PlayerPrefs.GetString(LineConfig.name);
                if (string.IsNullOrEmpty(usedBrushName))
                {
                    t.isOn = Brush == LineConfig.Brush;
                }
                else
                {
                    t.isOn = Brush.name == usedBrushName;
                    if (t.isOn)
                    {
                        LineConfig.Brush = Brush;
                    }
                }
            }

            StartCoroutine(LateInvokeEvent());
        }

        IEnumerator LateInvokeEvent()
        {
            yield return new WaitForEndOfFrame();
            if (OnBrushChanged != null) OnBrushChanged.Invoke(LineConfig.Brush.name);
        }

        #region IPointerClickHandler implementation

        public void OnPointerClick(PointerEventData eventData)
        {
            LineConfig.Brush = Brush;
            //PlayerPrefs.SetString(LineConfig.name, Brush.name);
            //PlayerPrefs.Save();

            if (OnBrushChanged != null) OnBrushChanged.Invoke(Brush.name);
        }

        #endregion

    }
}
