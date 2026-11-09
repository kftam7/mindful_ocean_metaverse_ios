using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterEffect : MonoBehaviour
{
    [SerializeField] protected Color normalColorl = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] protected Color underWatercolor = new Color(0.22f, 0.65f, 0.77f, 0.5f);
    [SerializeField] protected float lerpSpeed = 1f;

    protected Material defaultSkybox;
    protected Material noSkybox;

    protected bool isLerp = false;
    protected float lerpProgress = 0;

    protected virtual void Start()
    {
        RenderSettings.fog = true;
        defaultSkybox = RenderSettings.skybox;
//		Camera.main.backgroundColor = new Color(0, 0.4f, 0.7f, 1);
    }

    protected virtual void Update()
    {
        if (isLerp)
        {
            lerpProgress += Time.deltaTime * lerpSpeed;

            if (lerpProgress < 1)
            {
                RenderSettings.fogColor = Color.Lerp(origFogColor, targetFogColor, lerpProgress);
                RenderSettings.fogDensity = Mathf.Lerp(origFogDensity, targetFogDensity, lerpProgress);
            }
            else
            {
                RenderSettings.fogColor = targetFogColor;
                RenderSettings.fogDensity = targetFogDensity;
                RenderSettings.skybox = targetSkybox;
                isLerp = false;
            }
        }
    }

    public void SetNormalEffect()
    {
        RenderSettings.fogColor = normalColorl;
        RenderSettings.fogDensity = 0.002f;
        RenderSettings.skybox = defaultSkybox;
    }

    public void SetUnderwaterEffect()
    {
        RenderSettings.fogColor = underWatercolor;
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.skybox = noSkybox;
    }

    private Color origFogColor;
    private float origFogDensity;

    private Material targetSkybox;
    private Color targetFogColor;
    private float targetFogDensity;

    public void LerpNormalEffect()
    {
        targetFogColor = normalColorl;
        targetFogDensity = 0.002f;
        targetSkybox = defaultSkybox;

        lerpProgress = 0;
        isLerp = true;
    }

    public void LerpUnderwaterEffect()
    {
        targetFogColor = underWatercolor;
        targetFogDensity = 0.03f;
        targetSkybox = noSkybox;

        lerpProgress = 0;
        isLerp = true;
    }
}