using UnityEngine;

public class TopCanvas : MonoBehaviour
{
    public static TopCanvas Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TopCanvas>();
                if (instance == null)
                {
                    Canvas[] canvasGroup = FindObjectsOfType<Canvas>();
                    if (canvasGroup.Length > 0)
                    {
                        Canvas top = canvasGroup[0];
                        foreach (Canvas c in canvasGroup)
                        {
                            //***Should only name main canvas as Canvas
                            if (c.name == "Canvas")
                            {
                                top = c;
                                break;
                            }
                        }

                        instance = top.gameObject.AddComponent<TopCanvas>();

                        return instance;
                    }

                    Debug.LogWarning("No TopCanvas in scene. Added one.");
                    GameObject go = new GameObject();
                    go.name = "TopCanvas";

                    go.AddComponent<Canvas>();
                    instance = go.AddComponent<TopCanvas>();

                    return instance;
                }
            }

            return instance;
        }
    }

    private static TopCanvas instance;

    private void Awake()
    {
        instance = this;
    }

    private Transform canvasContainer;

    public Transform CanvasContainer
    {
        get
        {
            if (canvasContainer == null)
            {
#if UNITY_IOS
                iPhoneXTools.iPhoneXFix fix = this.gameObject.GetComponent<iPhoneXTools.iPhoneXFix>();
                if (fix != null)
                {
                    canvasContainer = fix.SafeArea;
                }
                else
                {
                    canvasContainer = this.transform;
                }
#else
                canvasContainer = this.transform;
#endif
            }

            return canvasContainer;
        }
    }
}