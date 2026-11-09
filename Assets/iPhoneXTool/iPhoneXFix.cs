using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace iPhoneXTools
{
    public class iPhoneXFix : MonoBehaviour
    {
#if UNITY_IOS
        public bool AutoSafeZone = true;
        public Rect SafeZone = new Rect(0, 0, 0, 0);

        [SerializeField]
        protected Transform safeArea;

        public Transform SafeArea
        {
            get
            {
                if (safeArea == null)
                {

                    CreateSafeArea();
                }

                return safeArea;
            }
        }

        private void Awake()
        {
            if (safeArea == null)
            {
                CreateSafeArea();
            }
        }

        public void CreateSafeArea()
        {

            bool enableTestMode = false;

#if UNITY_EDITOR
            enableTestMode = EditorPrefs.GetBool("IPHONEX_SAFE_ZONE", false);
#endif
            enableTestMode = true;

            if (SystemExtension.isIPhoneXScreen() || enableTestMode)
            {
                if (AutoSafeZone)
                {

                    SafeZone = (Screen.height > Screen.width) ? DefaultPortraitSafeZone : DefaultLandscapeSafeZone;
                }

                Canvas canvas = GetComponent<Canvas>();
                if (canvas != null)
                {

                    List<Transform> child = new List<Transform>();
                    foreach (Transform t in canvas.transform)
                    {

                        child.Add(t);
                    }

                    GameObject layer = new GameObject("SafeArea");

                    RectTransform rt = layer.AddComponent<RectTransform>();

                    safeArea = rt;

                    rt.SetParent(canvas.transform);

                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;

                    rt.anchoredPosition3D = Vector3.zero;
                    rt.localScale = Vector3.one;
                    rt.localRotation = Quaternion.identity;
                    rt.offsetMin = new Vector2(0, 0); // new Vector2(left, bottom);
                    rt.offsetMax = new Vector2(0, 0); // new Vector2(-right, -top);

                    foreach (Transform t in child)
                    {

                        t.SetParent(rt);
                    }

                    Vector2 s = new Vector2(SafeZone.size.x / 2f, SafeZone.size.y / 2f);

                    rt.offsetMin = s; // new Vector2(left, bottom);
                    rt.offsetMax = -s; // new Vector2(-right, -top)

                    rt.anchoredPosition3D = SafeZone.position;
                }
            }else{
                //For not iPhoneX
                safeArea = this.transform;
            }
        }

#if UNITY_EDITOR
        //Only use by editor
        public void AdjustSafeAreaByEditor()
        {

            RectTransform rt = this.transform.GetChild(0) as RectTransform;
            if (rt != null && rt.name == "SafeArea")
            {

                Vector2 s = new Vector2(SafeZone.size.x / 2f, SafeZone.size.y / 2f);

                rt.offsetMin = s; // new Vector2(left, bottom);
                rt.offsetMax = -s; // new Vector2(-right, -top)

                rt.anchoredPosition3D = SafeZone.position;
            }
        }
#endif

        public static readonly Rect DefaultLandscapeSafeZone = new Rect(0, 31, 260, 62);
        public static readonly Rect DefaultPortraitSafeZone = new Rect(0, -17, 0, 226);
#endif
    }
}
