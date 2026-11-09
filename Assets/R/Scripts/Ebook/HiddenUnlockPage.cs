using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenUnlockPage : MonoBehaviour
{
    public GameObject[] LockedStatusGameObject;
    public GameObject[] UnlockedStatusGameObject;

    public void SetStatus(bool isLocked)
    {
        foreach (GameObject go in LockedStatusGameObject)
        {
            go.SetActive(isLocked);
        }

        foreach (GameObject go in UnlockedStatusGameObject)
        {
            go.SetActive(!isLocked);
        }
    }
}