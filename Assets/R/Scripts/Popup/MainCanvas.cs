using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MainCanvas>();
                if (instance == null)
                {
                    Canvas[] canvasGroup = FindObjectsOfType<Canvas>();
                    if (canvasGroup.Length > 0)
                    {
                        Canvas main = canvasGroup[0];
                        foreach (Canvas c in canvasGroup)
                        {
                            //***Should only name main canvas as Canvas
                            if (c.name == "Canvas")
                            {
                                main = c;
                                break;
                            }
                        }

                        instance = main.gameObject.AddComponent<MainCanvas>();

                        return instance;
                    }
                    else
                    {
                        Debug.LogWarning("No Canvas in scene. Added one.");
                        GameObject go = new GameObject();
                        go.name = "Canvas";

                        go.AddComponent<Canvas>();
                        instance = go.AddComponent<MainCanvas>();

                        return instance;
                    }
                }
            }

            return instance;
        }
    }

    protected static MainCanvas instance;

    private void Awake()
    {
        instance = this;
    }

    protected Transform canvasContainer;

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