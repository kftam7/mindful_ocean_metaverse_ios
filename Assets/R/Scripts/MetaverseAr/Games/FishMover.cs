using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FishMover : MonoBehaviour
{
    [SerializeField] private Camera _arCamera;
    [SerializeField] private Transform fishMoveTarget;
    [SerializeField] private ParticleSystem sonarFx;
    [SerializeField] private GameObject moveUiIndicator;
    [SerializeField] private LayerMask bottlesLayer;

    [Header("Game Tips")]
    [SerializeField] private TipEvent tipEvent;

    private AukiSdkWrapper _aukiSdkWrapper;
    private float _initialDistance;
    private float _currentDistance;
    private Vector3 _targetPos;
    private Transform _ct;

    private RaycastHit[] _raycastHit = new RaycastHit[5];

    public static event Action<Vector3> OnTapMove;

    // Start is called before the first frame update
    void Start()
    {
        _aukiSdkWrapper = AukiSdkWrapper.Instance;
        GameController.OnStateChange += GameStateChange;

        moveUiIndicator.SetActive(false);

        _initialDistance = Vector3.Distance(_arCamera.transform.position, fishMoveTarget.position);

        _ct = _arCamera.transform;
    }

    void OnDestroy()
    {
        GameController.OnStateChange -= GameStateChange;
    }

    private void GameStateChange(ARGameState state)
    {
        moveUiIndicator.SetActive(state == ARGameState.GameInSession);
    }

    void Update()
    {
        if (!moveUiIndicator.activeInHierarchy) return;
        if (BottleController.SpawnedBottles == null) return;

        var tapped = false;
#if UNITY_EDITOR
        tapped = Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject();
#else
        tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject();
#endif
        RayToBottles();

        UpdateIndicatorToBottleDistance();

        if (tapped)
        {
            _targetPos = fishMoveTarget.position;
            TipEventTrigger.TriggerEvent(tipEvent);
            sonarFx.Play();
            MoveFish(_targetPos);
        }

        foreach (var bottle in BottleController.SpawnedBottles)
        {
            bottle.TestPosition(fishMoveTarget.position);
        }
    }

    private void UpdateIndicatorToBottleDistance()
    {
        var targetPos = _ct.position + _ct.forward * _currentDistance;
        fishMoveTarget.position = Vector3.Lerp(fishMoveTarget.position, targetPos, 5f * Time.deltaTime);

        var normalizedDistance = Mathf.InverseLerp(0.3f, _initialDistance, _currentDistance);
        var scale = Mathf.Lerp(2f, 0.75f, normalizedDistance);
        moveUiIndicator.transform.localScale = scale * Vector3.one;
    }

    private void RayToBottles()
    {
        var ray = _arCamera.ViewportPointToRay(0.5f * Vector3.one);
        var hits = Physics.RaycastNonAlloc(ray, _raycastHit, _initialDistance, bottlesLayer.value);

        for (int i = 0; i < hits; i++)
        {
            var hit = _raycastHit[i];

            if (hit.collider == null) continue;

            if (hit.collider.TryGetComponent<BottleScript>(out var bottle))
            {
                _currentDistance = hit.distance * 1.25f;
                return;
            }
        }

        _currentDistance = _initialDistance;
    }

    private void MoveFish(Vector3 pos)
    {
        Logger.Log("Tapped");
        OnTapMove?.Invoke(pos);
    }
}
