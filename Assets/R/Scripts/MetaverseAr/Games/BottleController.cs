using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using Game.Models;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Class for spawning bottles, should implement:
/// - Setup() method, to setup timer between spawns, maximal number spawn
/// - Spawning handling with bottle system
/// extract to interface later for networked version
/// </summary>
[Serializable]
public class BottleController
{
    [SerializeField] private BottleScript bottlePrefab;
    [SerializeField] private GameObject weedPrefab;
    [SerializeField] private RockScript rockPrefab;

    private int _maxBottleCount;
    private int _currentBottleCount;
    private BottleSystem _bottleSystem;
    private Session _session;
    private bool _isHost;
    private float _cameraBasedY;
    private List<Vector3> _positionsToAvoid = new();

    private readonly List<GameObject> _spawnedProps = new();
    public static List<BottleScript> SpawnedBottles { get; private set; } = new();

    public void Setup(int maxBottlesCount, BottleSystem bottleSystem, Session session, bool isHost)
    {
        _maxBottleCount = maxBottlesCount;
        _bottleSystem = bottleSystem;
        _session = session;
        _isHost = isHost;
        _currentBottleCount = 0;

        SpawnedBottles = new();

        BottleScript.OnPicked += OnPickBottle;
        _bottleSystem.OnBottleSpawn += SpawnBottle;
        _bottleSystem.OnBottleDestroy += DestroyBottle;

        if (Camera.main) _cameraBasedY = Camera.main.transform.position.y - 0.5f;

        PopulateBottles();
    }

    public void CleanUp()
    {
        BottleScript.OnPicked -= OnPickBottle;
        if (_bottleSystem != null)
        {
            _bottleSystem.OnBottleSpawn -= SpawnBottle;
            _bottleSystem.OnBottleDestroy -= DestroyBottle;
            _bottleSystem = null;
        }
        _session = null;
        CleanupBottles();
    }

    private void PopulateBottles()
    {
        if (!_isHost) return;
        for (int i = 0; i < _maxBottleCount; i++)
        {
            BroadcastSpawn();
        }
    }

    private void CleanupBottles()
    {
        _positionsToAvoid.Clear();

        foreach (var item in SpawnedBottles)
        {
            if (_isHost) _bottleSystem?.DeleteBottle(item.Entity);
            if (item) UnityEngine.Object.Destroy(item.gameObject);
        }

        SpawnedBottles.Clear();

        foreach (var item in _spawnedProps)
        {
            if (item) UnityEngine.Object.Destroy(item);
        }

        _spawnedProps.Clear();
    }

    private void BroadcastSpawn()
    {
        _currentBottleCount++;
        Logger.Log($"[Controller] Broadcast Spawn Bottle");
        var pose = new Pose()
        {
            position = GetRandomPos(_cameraBasedY),
            rotation = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.up)
        };

        var (rockPose, weedPose) = GetPropsPose(pose, 0.3f, ref _positionsToAvoid, 0.3f);

        var payload = new BottlePayload()
        {
            bottlePose = new SPose(pose),
            spawnProps = true,
            rockPose = new SPose(rockPose),
            weedPose = new SPose(weedPose),
            rockMeshId = Random.Range(0, rockPrefab.MeshesLength)
        };

        _session.AddEntity(e =>
        {
            Logger.Log($"[Controller] Call System AddBottle");
            _bottleSystem.AddBottle(e, payload);
        }, err=>Debug.LogError(err));
    }

    private void RespawnBottle()
    {
        _positionsToAvoid.Clear();

        foreach (var item in SpawnedBottles)
        {
            _positionsToAvoid.Add(item.transform.position);
        }

        foreach (var item in _spawnedProps)
        {
            _positionsToAvoid.Add(item.transform.position);
        }

        _currentBottleCount++;
        Logger.Log($"[Controller] Broadcast Spawn Bottle");
        var pose = new Pose()
        {
            position = GenerateNonOverlappingPosition(GetRandomPos(_cameraBasedY), 0.3f, _positionsToAvoid, 0.3f),
            rotation = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.up)
        };

        var payload = new BottlePayload()
        {
            bottlePose = new SPose(pose),
            spawnProps = false
        };

        _session.AddEntity(e =>
        {
            Logger.Log($"[Controller] Call System AddBottle");
            _bottleSystem.AddBottle(e, payload);
        }, err => Logger.LogError(err));
    }

    #region CALLBACK METHODS
    private void OnPickBottle(uint entityId, uint participantId, BottleScript bottle)
    {
        _bottleSystem.AddScore(entityId, participantId);
        _bottleSystem.DeleteBottle(bottle.Entity);
        if (SpawnedBottles.Contains(bottle)) SpawnedBottles.Remove(bottle);
    }

    private void SpawnBottle(Entity entity, BottlePayload payload)
    {
        Logger.Log($"[Controller] Spawn Bottle");
        var bottlePose = payload.bottlePose.GetPose();
        var bottle = UnityEngine.Object.Instantiate(bottlePrefab, bottlePose.position, bottlePose.rotation);
        bottle.SetEntity(entity);
        SpawnedBottles.Add(bottle);

        if (!payload.spawnProps) return;
        
        var rockPose = payload.rockPose.GetPose();
        var rock = UnityEngine.Object.Instantiate(rockPrefab, rockPose.position, rockPose.rotation);
        rock.Setup(payload.rockMeshId);
        var weedPose = payload.weedPose.GetPose();
        var weed = UnityEngine.Object.Instantiate(weedPrefab, weedPose.position, weedPose.rotation);
        _spawnedProps.Add(rock.gameObject);
        _spawnedProps.Add(weed);
    }

    private void DestroyBottle(uint entityId)
    {
        var bottle = SpawnedBottles.Find(x => x.Entity.Id.Equals(entityId));

        if (_isHost)
        {
            _currentBottleCount--;
            if (_currentBottleCount < _maxBottleCount)
                RespawnBottle();
        }

        if (bottle)
        {
            if (SpawnedBottles.Contains(bottle)) SpawnedBottles.Remove(bottle);
            bottle.DestroyFx();
            UnityEngine.Object.Destroy(bottle.gameObject);
        }
    }
    #endregion

    #region  HELPER METHODS

    private Vector3 GetRandomPos(float height)
    {
        var randomSpherePos = Random.insideUnitSphere;
        randomSpherePos.y = height;
        return randomSpherePos;
    }

    private (Pose, Pose) GetPropsPose(Pose bottlePose, float radius, ref List<Vector3> positionsToAvoid, float minDistance = 0.5f)
    {
        var center = bottlePose.position;
        
        positionsToAvoid.Add(center);

        Vector3 rockRandomPos = GenerateNonOverlappingPosition(center, radius, positionsToAvoid, minDistance);
        var rockPose = new Pose
        {
            position = rockRandomPos,
            rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)
        };

        positionsToAvoid.Add(rockRandomPos);

        Vector3 weedRandomPos = GenerateNonOverlappingPosition(center, radius, positionsToAvoid, minDistance);
        var weedPose = new Pose
        {
            position = weedRandomPos,
            rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up)
        };

        positionsToAvoid.Add(weedRandomPos);

        return (rockPose, weedPose);
    }

    private Vector3 GenerateNonOverlappingPosition(Vector3 center, float radius, List<Vector3> positionsToAvoid, float minDistance, int maxAttempts = 20)
    {
        Vector3 worldPos;
        int attempts = 0;
        bool positionValid;
        
        do
        {
            var randomPos = Random.insideUnitCircle.normalized * radius;
            worldPos = center + new Vector3(randomPos.x, 0f, randomPos.y);

            positionValid = true;
            foreach (var avoidPos in positionsToAvoid)
            {
                if (Vector3.Distance(worldPos, avoidPos) < minDistance)
                {
                    positionValid = false;
                    break;
                }
            }

            attempts++;

            if (attempts >= maxAttempts)
            {
                Logger.LogWarning("Could not find non-overlapping position after maximum attempts. Using fallback position.");
                worldPos = FindBestFallbackPosition(center, radius, positionsToAvoid);
                break;
            }

        } while (!positionValid);
        
        return worldPos;
    }

    private Vector3 FindBestFallbackPosition(Vector3 center, float radius, List<Vector3> positionsToAvoid)
    {
        if (positionsToAvoid.Count == 0)
        {
            var randomPos = Random.onUnitSphere * radius;
            randomPos.y = 0f;
            return center + randomPos;
        }
        
        Vector3 bestPosition = center;
        float maxMinDistance = 0f;
        
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 testPos = center + new Vector3(randomDir.x, 0f, randomDir.y) * radius;
            
            float minDistance = float.MaxValue;
            foreach (var avoidPos in positionsToAvoid)
            {
                float dist = Vector3.Distance(testPos, avoidPos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                }
            }
            
            if (minDistance > maxMinDistance)
            {
                maxMinDistance = minDistance;
                bestPosition = testPos;
            }
        }
        
        return bestPosition;
    }

    #endregion
}