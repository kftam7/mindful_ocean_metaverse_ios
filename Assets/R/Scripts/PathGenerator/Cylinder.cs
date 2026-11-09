using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder : Volume
{
    public Vector3 Center;
    public float Radius;
    public float Height;
    public Vector3 Rotation;

    //for rotate vector3
    protected Quaternion euler;

    public Cylinder(Vector3 center, float radius, float height, Vector3 rotation)
    {
        this.Center = center;
        this.Radius = radius;
        this.Height = height;
        this.Rotation = rotation;
        this.euler = Quaternion.Euler(rotation);
    }

    public Vector3 GetRandomPosition()
    {
        Vector2 v = Random.insideUnitCircle * Radius;

        Vector3 r = new Vector3(v.x, Random.Range(-Height / 2f, Height / 2f), v.y);

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