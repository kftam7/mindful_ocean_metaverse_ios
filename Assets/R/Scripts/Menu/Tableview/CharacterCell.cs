using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCell : MonoBehaviour
{
    void Start()
    {
        GetComponent<ModelContainer>().InitModelContainer(DataCenter.Instance.CharacterDataDic["Shark"]);
    }
}