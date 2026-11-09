using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using Auki.ConjureKit.ECS;
using Game.Models;
using UnityEngine;

public class BottleSystem : SystemBase
{
    private const string BottleComponent = "BOTTLE.COMPONENT";
    private const string PlayerScoreComponent = "SCORE.COMPONENT";
    private const string PlayerUsernameComponent = "USERNAME.COMPONENT";

    private uint _bottleComponentTypeId;
    private uint _scoreComponentTypeId;
    private uint _usernameComponentTypeId;
    // private byte[] emptyBytes;

    public event Action<Entity, BottlePayload> OnBottleSpawn;
    public event Action<uint> OnBottleDestroy;
    public event Action<uint, int> OnPlayerScore;
    public static event Action<uint, PlayerData> OnPlayerJoined;

    private List<Entity> _spawnedBottlesEntity = new();
    private Dictionary<uint, int> _playersScore = new();
    private Dictionary<uint, PlayerData> _playersData = new();

    public BottleSystem(Session session) : base(session)
    {
        // emptyBytes = Array.Empty<byte>();
    }

    public override string[] GetComponentTypeNames()
    {
        return new[] { BottleComponent, PlayerScoreComponent, PlayerUsernameComponent };
    }

    public void GetComponentsTypeId()
    {
        _session.GetComponentTypeId(BottleComponent, u => _bottleComponentTypeId = u, Debug.LogError);

        _session.GetComponentTypeId(PlayerScoreComponent, u => _scoreComponentTypeId = u, Debug.LogError);

        _session.GetComponentTypeId(PlayerUsernameComponent, u => _usernameComponentTypeId = u, Debug.LogError);
    }

    public void GetAllPreviousPlayers()
    {
        _session.GetComponents(_usernameComponentTypeId, result =>
        {
            foreach (var item in result)
            {
                var owner = _session.GetEntity(item.EntityId).ParticipantId;
                var data = item.Data.FromJsonByteArray<PlayerData>();
                _playersData.TryAdd(owner, data);
                OnPlayerJoined?.Invoke(data.participantId, data);
            }
        });
    }

    public override void Update(IReadOnlyList<(EntityComponent component, bool localChange)> updated)
    {
        foreach (var item in updated)
        {
            if (item.component.ComponentTypeId == _bottleComponentTypeId)
            {
                var entity = _session.GetEntity(item.component.EntityId);

                if (!_spawnedBottlesEntity.Contains(entity))
                {
                    var pose = item.component.Data.FromJsonByteArray<BottlePayload>();
                    OnBottleSpawn?.Invoke(entity, pose);
                    _spawnedBottlesEntity.Add(entity);
                }
            }

            if (item.component.ComponentTypeId == _scoreComponentTypeId)
            {
                var ownerId = item.component.Data.FromJsonByteArray<uint>();

                if (!_playersScore.TryAdd(ownerId, 0))
                {
                    _playersScore[ownerId]++;
                }

                OnPlayerScore?.Invoke(ownerId, _playersScore[ownerId]);
            }

            if (item.component.ComponentTypeId == _usernameComponentTypeId)
            {
                var data = item.component.Data.FromJsonByteArray<PlayerData>();

                if (!_playersData.ContainsKey(data.participantId))
                {
                    _playersData.Add(data.participantId, data);
                    OnPlayerJoined?.Invoke(data.participantId, data);
                }
            }
        }
    }

    public override void Delete(IReadOnlyList<(EntityComponent component, bool localChange)> deleted)
    {
        foreach (var item in deleted)
        {
            OnBottleDestroy?.Invoke(item.component.EntityId);
        }
    }

    public void AddBottle(Entity entity, BottlePayload bottlePayload)
    {
        var payload = bottlePayload.ToJsonByteArray();
        _session.AddComponent(_bottleComponentTypeId, entity.Id, payload, null, Debug.LogError);
    }

    public void InitializeScore(uint entityId, uint playerId)
    {
        var data = playerId.ToJsonByteArray();

        _session.AddComponent(_scoreComponentTypeId, entityId, data, null, Debug.LogError);
    }

    public void AddScore(uint entityId, uint playerId)
    {
        var data = playerId.ToJsonByteArray();

        _session.UpdateComponent(_scoreComponentTypeId, entityId, data);
    }

    public void AddPlayerData(uint entityId, PlayerData playerData)
    {
        var data = playerData.ToJsonByteArray();
        _session.AddComponent(_usernameComponentTypeId, entityId, data, null, Debug.LogError);
    }

    public void DeleteBottle(Entity entity)
    {
        if (_session == null)
        {
            Logger.LogWarning($"Session is closed, cancel deleting entity {entity.Id}");
            return;
        }
        _session.DeleteComponent(_bottleComponentTypeId, entity.Id, null, Debug.LogError);
    }

    public void DeleteScoreComponent(Entity entity)
    {
        if (_session == null)
        {
            Logger.LogWarning($"Session is closed, cancel deleting entity {entity.Id}");
            return;
        }
        _session.DeleteComponent(_scoreComponentTypeId, entity.Id, null, Debug.LogError);
    }
    
    public void DeletePlayerData(uint entityId)
    {
        if (_session == null)
        {
            Logger.LogWarning($"Session is closed, cancel deleting entity {entityId}");
            return;
        }
        _session.DeleteComponent(_usernameComponentTypeId, entityId, null, Debug.LogError);
    }
}