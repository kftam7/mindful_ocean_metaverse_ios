using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drip : MonoBehaviour
{
    [SerializeField] private GameObject Drip;

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(Drip, new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z), Quaternion.identity);
    }
}