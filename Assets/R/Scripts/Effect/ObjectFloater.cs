using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFloater : MonoBehaviour
{
    [SerializeField] protected float objectYOffset = 0;

    protected Vector3 closestVertice;

    // Update is called once per frame
    void Update()
    {
        closestVertice = SeaPlane.Instance.GetClosestVertice(this.transform.position);
        this.transform.position = new Vector3(this.transform.position.x, closestVertice.y + objectYOffset, this.transform.position.z);
    }
}