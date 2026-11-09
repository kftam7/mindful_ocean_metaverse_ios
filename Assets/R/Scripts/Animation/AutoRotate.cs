using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    // Update is called once per frame
    [SerializeField] float speed = 60;
    [SerializeField] int direction = 1;

    void Update()
    {
        this.transform.Rotate(new Vector3(0, Time.deltaTime * speed * direction, 0));
    }
}