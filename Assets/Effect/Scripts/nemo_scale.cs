using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nemo_scale : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        float rand;
        rand = Random.Range(0.1f, 0.05f);
        transform.localScale = new Vector3(rand, rand, rand);
    }

    // Update is called once per frame
    void Update() { }
}