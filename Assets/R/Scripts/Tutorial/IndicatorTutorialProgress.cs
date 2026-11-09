using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class IndicatorTutorialProgress : GeneralTutorialProgress
{
    [SerializeField] protected bool isRotateBaseOnDirection = true;
    [SerializeField] protected Text message;
    [SerializeField] protected RectTransform textBG;
    [SerializeField] protected Vector2 textBGSizeOffset = new Vector2(150, 30);

    public override void StartProgress(TutorialStep step)
    {
        base.StartProgress(step);

        for (int i = 0; i < step.Target.Length; i++)
        {
            GameObject target = GameObject.Find(mCurrentTutorialProgress.Target[i]);

            if (target == null)
            {
                //Try to search it again from root
                Transform result = MainCanvas.Instance.CanvasContainer.FindRecursive(mCurrentTutorialProgress.Target[i]);
                if (result != null)
                {
                    target = result.gameObject;
                }
            }

            if (target != null)
            {
                GameObject d = GameObject.Instantiate(target);
                d.name = target.name;
                d.transform.SetParent(this.transform);
                //FIXME
                //Temp method only, please fix all the UI to scale 1
                //Calculate scalte recursively
                Transform scaleTarget = target.transform;
                Vector3 scale = Vector3.one;
                while (scaleTarget != null && !scaleTarget.GetComponent<Canvas>())
                {
                    scale = Vector3.Scale(scale, scaleTarget.localScale);
                    scaleTarget = scaleTarget.parent;
                }

                d.transform.localScale = scale;
                d.transform.rotation = target.transform.rotation;
                d.transform.position = target.transform.position;

                //Debug.Log(target.transform.position);
                //Debug.Log(d.transform.position);

                if (d.GetComponent<Animator>())
                {
                    DestroyImmediate(d.GetComponent<Animator>());
                }

                CanvasGroup c = target.GetComponent<CanvasGroup>();
                if (c == null)
                {
                    c = target.AddComponent<CanvasGroup>();
                }

                c.alpha = 0;

                TutorialProgressEndCallback.AddListener((TutorialStep data) =>
                {
                    c.alpha = 1;
                });

                //Button targetButton = GeneralTools.GetTargetByName<Button>(d, mCurrentTutorialProgress.TargetButton[i]);
                Button b = mCurrentTutorialProgress.TargetButton[i] != "na" ? GeneralTools.GetTargetByName<Button>(d, mCurrentTutorialProgress.TargetButton[i]) : null;
                if (b)
                {
                    if (!string.IsNullOrEmpty(mCurrentTutorialProgress.PointerPrefab[i]) && mCurrentTutorialProgress.PointerPrefab[i] != "na")
                    {
                        GameObject indicator = GetIndicator(mCurrentTutorialProgress.PointerPrefab[i]);
                        indicator.transform.position = GeneralTools.GetPositon(step.Direction[i], b.transform as RectTransform);
                        indicator.transform.localScale = scale;
                        if (isRotateBaseOnDirection) indicator.transform.localEulerAngles += new Vector3(0, 0, 90f * (int)step.Direction[i]);
                        indicator.transform.SetAsLastSibling();
                    }

                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(GeneralTools.GetTargetByName<Button>(target, mCurrentTutorialProgress.TargetButton[i]).onClick.Invoke);
                    b.onClick.AddListener(OnTutorialProgressEnd);
                    b.interactable = true;
                }

                if (step.BlockRaycast[i])
                {
                    Graphic[] gs = d.GetComponentsInChildren<Graphic>();
                    foreach (var g in gs)
                    {
                        g.raycastTarget = false;
                    }
                }
            }
            else
            {
                Debug.LogError("Cannot find tutorial target- " + mCurrentTutorialProgress.Target[i]);
            }
        }

        FillMessage(step);
    }

    public virtual void FillMessage(TutorialStep step)
    {
        if (message)
        {
            message.text = (!string.IsNullOrEmpty(step.Description) && step.Description != "-") ? ProGamer.Localization.Get(step.Description) : "";

            if (step.SetAlignment)
            {
                float textAlignment = Mathf.Clamp01(step.TextAlignment); //prevent user input error

                message.rectTransform.pivot = new Vector2(0.5f, textAlignment);
                message.rectTransform.anchorMin = new Vector2(0, textAlignment);
                message.rectTransform.anchorMax = new Vector2(1, textAlignment);
                message.rectTransform.anchoredPosition3D = Vector3.zero;
            }

            if (step.IsBestFit)
            {
                message.resizeTextForBestFit = step.IsBestFit;
                message.resizeTextMaxSize = step.FontSize;
                message.resizeTextMinSize = 1;
                message.fontSize = step.FontSize;
            }
            else
            {
                message.resizeTextForBestFit = step.IsBestFit;
                message.fontSize = step.FontSize;
            }

            if (textBG)
            {
                textBG.gameObject.SetActive(step.EnableTextBG);
                if (step.EnableTextBG)
                {
                    textBG.SetAsLastSibling();
                    StartCoroutine(LateUpdateBGUI());
                }
            }

            message.color = step.ColorForText;
            message.rectTransform.SetAsLastSibling();
            textBGSizeOffset = new Vector2(step.BGXOffset, step.BGYOffset);
        }
    }

    IEnumerator LateUpdateBGUI()
    {
        yield return new WaitForEndOfFrame();

        Vector2 bgSize = message.rectTransform.GetSize() * 1.1f;

        textBG.sizeDelta = bgSize + textBGSizeOffset;
        Vector2 pDif = textBG.pivot - message.rectTransform.pivot;

        textBG.localPosition = message.rectTransform.localPosition + new Vector3(bgSize.x * pDif.x, bgSize.y * pDif.y);
    }

    public virtual GameObject GetIndicator(string prefab)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(TutorialController.PrefabPath + prefab) as GameObject, this.transform);
        RectTransform rect = go.transform as RectTransform;

        rect.localScale = Vector3.one;
        rect.anchoredPosition3D = Vector3.zero;
        rect.SetAsLastSibling();
        return go;
    }
}