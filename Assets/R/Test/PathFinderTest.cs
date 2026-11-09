using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinderTest : MonoBehaviour
{
    public bool[,] Map = new bool[7, 7]
    {
        { true, false, true, true, true, true, true },
        { true, false, true, true, true, true, true },
        { true, true, true, true, true, true, true },
        { true, false, true, true, true, true, true },
        { true, true, false, true, true, true, true },
        { true, true, true, false, true, true, true },
        { true, true, true, true, true, true, true }
    };

    public Vector2Int StartPos;

    public Vector2Int EndPos;

    public void Find()
    {
        int[,] cost = PathFinder.CostCalculate(Map, StartPos, EndPos);


        for (int i = 0; i < cost.GetLength(0); i++)
        {
            string line = "";
            for (int r = 0; r < cost.GetLength(1); r++)
            {
                line += "-\t" + cost[i, r];
            }

            Debug.Log(line);
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 100, 100, 50), "Find"))
        {
            Find();
        }
    }
}