using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFish : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponentInParent<SpawnOnPlane>().NumOfFish--;
        //SpawnOnPlane.Instance.NumOfFish--;
        Destroy(other.transform.parent.gameObject);
    }
}