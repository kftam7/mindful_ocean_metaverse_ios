using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightConstraint : MonoBehaviour
{
    // Use this for initialization
    public GameObject obj;
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (obj != null)
            transform.position = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
    }
}