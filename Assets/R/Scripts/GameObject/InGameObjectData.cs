using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameObjectData : RandomObject
{
    public string ModelName { get; set; }
    public string Path { get; set; }
    public string ObjectType { get; set; }
    public string Effect { get; set; }
    public int EffectParameter { get; set; }
    public string EffectPrefab { get; set; }
    public float[] Possibilities { get; set; }
    public float[] DemoPossibilities { get; set; }

    public float ExistTime { get; set; }

    public float GetPossibility(int i)
    {
#if EVENT_DEMO
		return DemoPossibilities[Mathf.Clamp(i, 0, DemoPossibilities.Length -1)];
#else
        return Possibilities[Mathf.Clamp(i, 0, Possibilities.Length - 1)];
#endif
    }
}