using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineObjectScaleUpdater : MonoBehaviour
{
    public string TargetOutline = "Toon/Size Relative Outline";
    protected Material mOutlineMaterial;

    // Use this for initialization
    void Start()
    {
        Material[] ms = GetComponent<Renderer>().materials;
        foreach (Material m in ms)
        {
            if (m.shader.name == TargetOutline)
            {
                mOutlineMaterial = m;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mOutlineMaterial != null)
        {
            mOutlineMaterial.SetFloat("_ObjectScale", this.transform.lossyScale.x);
        }
    }
}