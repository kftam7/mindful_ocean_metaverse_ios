using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelContainer : MonoBehaviour
{
    protected CharacterData data;

    public CharacterData Data
    {
        get { return data; }
    }

    public void InitModelContainer(CharacterData data)
    {
        this.data = data;
        ProGamer.ReuseableObjectManager.Instantiate(data.PrefabPath, this.transform);
    }
}