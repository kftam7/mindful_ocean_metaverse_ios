using System.Collections.Generic;
using UnityEngine;

public class FishNamesView : MonoBehaviour
{
    [SerializeField] private RectTransform nameContainer;
    [SerializeField] private FishNamesComp nameTextPrefab;
    [SerializeField] private float yOffset = 0.1f;

    private RectTransform _parentCanvas;
    private Camera _camera;

    private Dictionary<uint, FishNamesComp> _spawnedNames = new();
    private Dictionary<uint, FishEntity> _fishes = new();
    private bool _showNames;

    void Start()
    {
        GameController.OnStateChange += OnStateChange;
        ARMetaverseManager.OnFishAddedToServer += FishAdded;
        AukiSdkWrapper.Instance.LeftEvent += LeftEvent;
        AukiSdkWrapper.Instance.PeerLeftEvent += OnPlayerLeft;

        _camera = Camera.main;
        _parentCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    void OnDestroy()
    {
        GameController.OnStateChange -= OnStateChange;
        ARMetaverseManager.OnFishAddedToServer -= FishAdded;
        AukiSdkWrapper.Instance.LeftEvent -= LeftEvent;
        AukiSdkWrapper.Instance.PeerLeftEvent -= OnPlayerLeft;
    }

    private void OnStateChange(ARGameState state)
    {
        _showNames = state is ARGameState.Waiting or ARGameState.GameInSession && !GameController.IsSinglePlayer;
        switch (state)
        {
            case ARGameState.GameEnd:
                foreach (var item in _spawnedNames)
                {
                    Destroy(item.Value.gameObject);
                }

                _spawnedNames.Clear();
                _fishes.Clear();
                break;
        }
    }

    private void LeftEvent()
    {
        OnStateChange(ARGameState.GameEnd);
    }

    //this methods listens whenever fish is added to the server
    private void FishAdded(FishEntity fishEntity)
    {
        if (_fishes.TryAdd(fishEntity.AukiEntity.ParticipantId, fishEntity))
        {
            OnPlayerJoined(fishEntity.AukiEntity.ParticipantId, fishEntity.Owned ? "You" : fishEntity.OwnerName);
        }
    }

    private void OnPlayerJoined(uint playerId, string playerName)
    {
        if (_spawnedNames.ContainsKey(playerId)) return;

        var name = Instantiate(nameTextPrefab, nameContainer);
        name.gameObject.SetActive(true);
        name.SetName(playerName);
        _spawnedNames.Add(playerId, name);
    }

    private void OnPlayerLeft(uint playerId)
    {
        if (_spawnedNames.TryGetValue(playerId, out var name))
        {
            Destroy(name.gameObject);
        }

        _spawnedNames.Remove(playerId);
        _fishes.Remove(playerId);
    }

    void Update()
    {
        if (_spawnedNames.Count <= 0) return;

        foreach (var item in _spawnedNames)
        {
            if (!_fishes.TryGetValue(item.Key, out var fish))
            {
                item.Value.SetVisibility(false);
                continue;
            }

            var fishPos = fish.transform.position;

            Vector2 screenPos = _camera.WorldToScreenPoint(fishPos + (yOffset * Vector3.up));
            screenPos.x *= _parentCanvas.rect.width / _camera.pixelWidth;
            screenPos.y *= _parentCanvas.rect.height / _camera.pixelHeight;
            item.Value.SetAnchoredPosition(screenPos - _parentCanvas.sizeDelta / 2f);

            var visible = IsFrontOfCamera(fishPos) && _showNames;
            item.Value.SetVisibility(visible);
        }
    }

    private bool IsFrontOfCamera(Vector3 pos)
    {
        return _camera.WorldToViewportPoint(pos).z > 0f;
    }
}