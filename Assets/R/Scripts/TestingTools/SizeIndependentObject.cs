using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SizeIndependentObject : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        float dis = (Camera.main.transform.position - this.transform.position).magnitude;
        float fov = Camera.main.fieldOfView;
        float m = (dis / (2 * Mathf.Tan(Mathf.PI * fov / 360)));

        this.transform.localScale = Vector3.one * m;
    }
}