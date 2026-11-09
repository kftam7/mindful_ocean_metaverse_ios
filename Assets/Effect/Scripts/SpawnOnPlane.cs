using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnPlane : MonoBehaviour
{
    [SerializeField] private GameObject fish;

    [SerializeField] private Transform door;

    //Set Boundaries for the plane
    //Add [SerializeField] in front if you want to edit in Inspector
    private float xMin = -0.15f;
    private float xMax = 0.15f;
    private float yMin = 0.4f;
    private float yMax = 1.05f;

    //set starting zPos
    private float zPos = 0.0f;

    //set how many fish you want to have at once
    private int fishLimit = 25;
    private int numOfFish = 0;

    public int NumOfFish
    {
        get { return numOfFish; }
        set { numOfFish = value; }
    }

    private void Start()
    {
        StartCoroutine("SpawnFish");
    }

    private float RandomNumber(float min, float max)
    {
        float num = (float)Random.Range(min, max);
        return num;
    }

    private IEnumerator SpawnFish()
    {
        while (true)
        {
            if (numOfFish < fishLimit)
            {
                float x = RandomNumber(xMin, xMax);
                float y = RandomNumber(yMin, yMax);
                GameObject go = Instantiate(fish, new Vector3(0f, 0f, 0f), this.transform.rotation, door);
                go.transform.localPosition = new Vector3(x, y, zPos);
                numOfFish++;
            }

            float randomSpawnTime = RandomNumber(0.5f, 1.0f);
            yield return new WaitForSeconds(randomSpawnTime);
        }
        //set how many seconds to wait check for fish
    }
}