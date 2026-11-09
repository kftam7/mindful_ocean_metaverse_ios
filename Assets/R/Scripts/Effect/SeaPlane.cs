using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//For square plane
[DisallowMultipleComponent]
public class SeaPlane : SceneInstance<SeaPlane>
{
    protected Cloth plane;
    protected Vector3 mFirstVertice;
    protected Vector3 mLastVertice;
    protected int dimension;
    protected int verticesLength;

    void Start()
    {
        plane = GetComponent<Cloth>();
        mFirstVertice = this.transform.TransformPoint(plane.vertices[0]);
        mLastVertice = this.transform.TransformPoint(plane.vertices[plane.vertices.Length - 1]);

        verticesLength = plane.vertices.Length;
        dimension = (int)Mathf.Sqrt(verticesLength);
    }

    public Vector3 GetClosestVertice(Vector3 p)
    {
        int x = Mathf.RoundToInt((p.x - mFirstVertice.x) / (mLastVertice.x - mFirstVertice.x) * (dimension - 1));
        int z = Mathf.RoundToInt((p.z - mFirstVertice.z) / (mLastVertice.z - mFirstVertice.z) * (dimension - 1));

        int index = Mathf.Max(0, Mathf.Min(verticesLength - 1, x + z * dimension));
        return this.transform.TransformPoint(plane.vertices[index]);
    }
}