using System;
using Auki.ConjureKit;
using ProGamer;
using UnityEngine;

public class BottleScript : MonoBehaviour
{
    [SerializeField] private RandomMesh randomMesh = new();
    [SerializeField] private GameObject pickupFx;
    [SerializeField] private AudioSource audioSource;

    [Header("Game Tips")]
    [SerializeField] private TipEvent tipEvent;

    private Entity _entity;
    private Transform _t;
    private Material _m;
    private Collider _c;
    private bool _picked;
    public static Action<uint, uint, BottleScript> OnPicked;

    public Entity Entity => _entity;

    public Vector3 Position => _t.position;

    private readonly int _mainColorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        _t = transform;
        _c = GetComponent<Collider>();
        randomMesh.Initialize(out _m);
    }

    public void SetEntity(Entity entity)
    {
        _entity = entity;
    }

    private void Picked(uint entityId, uint participantId)
    {
        OnPicked?.Invoke(entityId, participantId, this);
        TipEventTrigger.TriggerEvent(tipEvent);
        DestroyFx();
        Destroy(gameObject);
    }

    public void TestPosition(Vector3 position)
    {
        var withinRange = (transform.position - position).sqrMagnitude < _c.bounds.extents.sqrMagnitude;
        _m.SetColor(_mainColorId, withinRange ? Color.green : Color.white);
    }

    public void DestroyFx()
    {
        audioSource.transform.SetParent(null);
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length * 1.1f);
        pickupFx.transform.SetParent(null);
        pickupFx.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<FishEntity>(out var fish))
        {
            var participantId = fish.AukiEntity.ParticipantId;

            if (participantId != AukiSdkWrapper.Instance.MyPeerId)
                return;

            Picked(fish.AukiEntity.Id, participantId);
        }
    }
}