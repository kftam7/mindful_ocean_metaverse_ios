using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] protected float spawnTime = 1f;
    [SerializeField] protected int initSpawnCount = 5;
    [SerializeField] protected int spawnMax = 5;

    protected float spawnDistance = 5; //from collider raduis
    protected bool isEnable = true;
    protected int spawnCount = 0;

    List<InGameObjectData> dataList;

    void Start()
    {
        var collider = GetComponent<CircleCollider2D>();

        if (collider != null)
        {
            spawnDistance = collider.radius;
            collider.enabled = false;
        }

        dataList = new List<InGameObjectData>(DataCenter.Instance.InGameObjectDataDic.Values);

        for (int i = 0; i < initSpawnCount; i++)
        {
            RandomSpawn();
        }
    }

    public int GetVacancy()
    {
        return spawnMax - transform.childCount;
    }

    float timeCount = 0;

    void Update()
    {
        timeCount += Time.deltaTime;

        if (timeCount >= spawnTime)
        {
            if (isEnable && GetVacancy() > 0)
            {
                RandomSpawn();
            }

            timeCount = 0;
        }
    }

    void OnBecameVisible()
    {
        isEnable = false;
    }

    void OnBecameInvisible()
    {
        isEnable = true;
    }

    public void RandomSpawn()
    {
        SpawnObject(dataList.GetRandomObject<InGameObjectData>(Mathf.RoundToInt(InGameData.Instance.WholeTime / 60)));
    }

    public void SpawnObject(InGameObjectData data)
    {
        GameObject go = ProGamer.ReuseableObjectManager.Instantiate(data.Path, this.transform);
        InGameObject igo = go.AddComponent<InGameObject>();

        igo.Init(data);
        go.transform.eulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
        Vector2 r = Random.insideUnitCircle * spawnDistance;

        go.transform.position = this.transform.position + new Vector3(r.x, r.y);
    }
}