using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public class Cube : Volume
{
    public Vector3 Center;
    public Vector3 Size;

    public Vector3 Rotation;

    //for rotate vector3
    protected Quaternion euler;

    public Cube(Vector3 center, Vector3 size, Vector3 rotation)
    {
        this.Center = center;
        this.Size = size;
        this.Rotation = rotation;
        this.euler = Quaternion.Euler(rotation);
    }

    public Vector3 GetRandomPosition()
    {
        Vector3 r = new Vector3(Random.Range(-Size.x / 2f, Size.x / 2f), Random.Range(-Size.y / 2f, Size.y / 2f), Random.Range(-Size.z / 2f, Size.z / 2f));

        return this.euler * r + Center;
    }

    public Vector3 GetRandomPosition(float p)
    {
        return GetRandomPosition();
    }

    public Vector3 GetRandomPosition(float p, float useless)
    {
        return GetRandomPosition();
    }

    public bool IsInside(Vector3 position)
    {
        return true;
    }
}