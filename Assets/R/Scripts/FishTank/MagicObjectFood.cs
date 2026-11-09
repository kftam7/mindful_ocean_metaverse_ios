using System.Collections;
using UnityEngine;

public class MagicObjectFood : MagicObject
{
    [SerializeField] private float speedFactor = 1.5f;
    private bool hasBeenTouched;
    public float SpeedFactor => speedFactor;

    protected override void Awake()
    {
        base.Awake();
        Type = MagicObjectTypeEnum.Heart;
    }

    public void BeTouched()
    {
        if (AukiSdkWrapper.Instance.HasJoined && !AukiSdkWrapper.Instance.IsHost) return;
        if (!hasBeenTouched)
        {
            hasBeenTouched = true;
            StartCoroutine(StartToDestroySelf());
        }
    }

    private IEnumerator StartToDestroySelf()
    {
        yield return new WaitForSeconds(DisappearDelay);
        StartCoroutine(CheckToDisappear());
    }
}