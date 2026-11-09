using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TutorialController : ProGamer.Singleton<TutorialController>
{
    public const string PrefabPath = "Prefabs/Tutorial/";

    public Transform Container
    {
        get
        {
            if (!TutorialContainer.Instance)
            {
                GameObject go = new GameObject("TutorialContainer");

                go.transform.SetParent(MainCanvas.Instance.CanvasContainer);
                go.AddComponent<RectTransform>();
                go.AddComponent<TutorialContainer>();

                RectTransform rect = go.transform as RectTransform;

                if (rect != null)
                {
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition3D = Vector3.zero;

                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;

                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.one;

                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.SetAsLastSibling();
                }
            }

            return TutorialContainer.Instance.transform;
        }
    }

    public void StartTutorial(string name, string json)
    {
        GameSave.LastSelectedColoringPageCellIndex = 0;
        GameSave.LastSelectedPackageCellIndex = 0;
        GameSave.LastSelectedAquariumCellIndex = 0;


        GameObject go = new GameObject();
        go.name = name;
        TutorialFlowHandler handler = go.AddComponent<TutorialFlowHandler>();

        List<TutorialStep> steps = new List<TutorialStep>(JsonConvert.DeserializeObject<Dictionary<string, TutorialStep>>(json).Values);
        steps.Sort(delegate(TutorialStep x, TutorialStep y)
        {
            return x.Step.CompareTo(y.Step);
        });

        handler.InitTutorial(name, steps);
        handler.StartTutorial(true);

        DontDestroyOnLoad(go);
    }
}