using System.Collections;
using Auki.ConjureKit;
using UnityEngine;

public enum MagicObjectTypeEnum
{
    Heart,
    Torus,
    Bowl,
    Food
}

public class MagicObject : AukiObjectBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private float disappearDelay = 3f; // after being touched or being created
    public Transform CachedTransform { get; private set; }
    public Entity AukiEntity { get; set; }
    public bool IsDisappearing { get; private set; }
    public float DisappearDelay => disappearDelay;

    public MagicObjectTypeEnum Type { get; protected set; }

    protected virtual void Awake()
    {
        CachedTransform = transform;
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
        if (ARMetaverseManager.Instance != null)
        {
            ARMetaverseManager.Instance.RemoveMagicObjectFromList(this);
        }

        Debug.Log($"[{GetType().Name}] Magic object destroyed - {Type}");
    }

    public IEnumerator CheckToDisappear()
    {
        Debug.Log($"[{GetType().Name}] CheckToDisappear - {Type}");
        if (!AukiSdkWrapper.Instance.HasJoined)
        {
            StartCoroutine(Disappear());
        }
        else
        {
            if (!AukiSdkWrapper.Instance.IsHost) yield break;
            // wait before adding entity to server is completed.
            while (AukiEntity == null)
            {
                yield return null;
            }

            if (AukiEntity.ParticipantId == AukiSdkWrapper.Instance.MyPeerId) // created by host
            {
                AukiSdkWrapper.Instance.DeleteEntity(AukiEntity.Id);
                StartCoroutine(Disappear());
            }
            else
            {
                AukiSdkWrapper.Instance.RequestDeleteEntityAction(AukiEntity.Id);
            }
        }
    }

    public IEnumerator Disappear()
    {
        IsDisappearing = true;
        animator.CrossFade("Disappear", 0);
        yield return new WaitForSeconds(2);
        Debug.Log($"[{GetType().Name}] Disappear : Destroy {gameObject.name}");
        Destroy(gameObject);
    }
}