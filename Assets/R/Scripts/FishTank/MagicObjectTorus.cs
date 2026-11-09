using System.Collections;
using UnityEngine;

public class MagicObjectTorus : MagicObject
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float speedFactor = 1.5f;
    [SerializeField] private float amp = 1f;
    [SerializeField] private float freq = 1f;
    public float Radius => radius;
    public float SpeedFactor => speedFactor;
    public float Amp => amp;
    public float Freq => freq;

    protected override void Awake()
    {
        base.Awake();
        Type = MagicObjectTypeEnum.Torus;
    }

    public void DestroySelf()
    {
        if (AukiSdkWrapper.Instance.HasJoined && !AukiSdkWrapper.Instance.IsHost) return;
        StartCoroutine(StartToDestroySelf());
    }

    private IEnumerator StartToDestroySelf()
    {
        yield return new WaitForSeconds(DisappearDelay);
        StartCoroutine(CheckToDisappear());
    }
}