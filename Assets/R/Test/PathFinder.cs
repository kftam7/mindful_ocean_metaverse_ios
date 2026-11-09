using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    public static int[,] CostCalculate(bool[,] map, Vector2Int start, Vector2Int end)
    {
        Vector2Int bound = new Vector2Int(map.GetLength(0), map.GetLength(1));

        int[,] costMap = new int[bound.x, bound.y];

        int max = bound.x * bound.y;

        for (int i = 0; i < bound.x; i++)
        {
            for (int r = 0; r < bound.y; r++)
            {
                costMap[i, r] = max;
            }
        }

        costMap[start.x, start.y] = 0;

        List<Vector2Int> searchList = new List<Vector2Int>();

        searchList.Add(start);

        for (int i = 0; i < searchList.Count; i++)
        {
            //get target
            Vector2Int target = searchList[i];

            //find around
            List<Vector2Int> around = new List<Vector2Int>()
            {
                target + Vector2Int.up,
                target + Vector2Int.down,
                target + Vector2Int.left,
                target + Vector2Int.right
            };

            int cost = costMap.GetValue(target) + 1;

            //search around
            foreach (var pos in around)
            {
                //check bounded
                if (pos.IsBounded(bound) && map.GetValue(pos) && costMap.GetValue(pos) > cost)
                {
                    costMap[pos.x, pos.y] = cost;
                    searchList.Add(pos);
                }

                if (pos == end)
                {
                    return costMap;
                }
            }
        }

        return costMap;
    }

    public static T GetValue<T>(this T[,] array, Vector2Int pos)
    {
        return array[pos.x, pos.y];
    }
}