using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tube : Volume
{
    //for rotate vector3
    protected Quaternion euler;

    public Vector3 Center;
    public float Radius;
    public float InnerRadius;
    public float Height;
    public Vector3 Rotation;

    public Tube(Vector3 center, float radius, float innerRadius, float height, Vector3 rotation)
    {
        this.Center = center;
        this.Radius = radius;
        this.InnerRadius = innerRadius;
        this.Height = height;
        this.Rotation = rotation;
        this.euler = Quaternion.Euler(rotation);
    }

    public Vector3 GetRandomPosition()
    {
        float angle = Random.Range(0f, 360f);

        return GetRandomPosition(angle);
    }

    public Vector3 GetRandomPosition(float byAngle)
    {
        float d = Random.Range(InnerRadius, Radius);

        Vector2 n = Quaternion.Euler(0, 0, byAngle) * Vector2.right * d;

        Vector3 r = new Vector3(n.x, Random.Range(-Height / 2f, Height / 2f), n.y);

        return this.euler * r + Center;
    }

    public Vector3 GetRandomPosition(float byAngle, float distance)
    {
        Vector2 n = Quaternion.Euler(0, 0, byAngle) * Vector2.right * distance;

        Vector3 r = new Vector3(n.x, Random.Range(-Height / 2f, Height / 2f), n.y);

        return this.euler * r + Center;
    }

    public bool IsInside(Vector3 position)
    {
        return true;
    }
}