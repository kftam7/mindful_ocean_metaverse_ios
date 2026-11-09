using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PickRandomObject
{
    public static T GetRandomObject<T>(this List<T> list, int dataSet) where T : RandomObject
    {
        float sum = 0;
        foreach (T r in list)
        {
            sum += r.GetPossibility(dataSet);
        }

        float n = Random.Range(0, sum);

        float count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            count += list[i].GetPossibility(dataSet);
            if (count >= n)
            {
                return list[i];
            }
        }

        return list[list.Count - 1];
    }

    private static System.Random random = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public interface RandomObject
{
    float GetPossibility(int i);
}