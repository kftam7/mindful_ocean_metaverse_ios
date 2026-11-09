using System.Collections;
using UnityEngine;

public class MagicObjectBowl : MagicObject
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float speedFactor = 1.5f;
    [SerializeField] private float ySpeedFactor = 0.1f;
    public float Radius => radius;
    public float SpeedFactor => speedFactor;
    public float YSpeedFactor => ySpeedFactor;

    protected override void Awake()
    {
        base.Awake();
        Type = MagicObjectTypeEnum.Bowl;
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