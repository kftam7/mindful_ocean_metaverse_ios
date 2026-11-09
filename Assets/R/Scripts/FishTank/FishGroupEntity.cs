using UnityEngine;

public class FishGroupEntity : FishEntity
{
    public override void SetSkin(Texture skin)
    {
        if (skin != null)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.material.SetTexture("_MainTex", skin);
        }
    }

    public override void PlayAnimation(string animation)
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators) a.CrossFade(animation, 0f);
    }

    public override void SetTimeScale(float timeScale)
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators) a.speed = timeScale;
    }
}