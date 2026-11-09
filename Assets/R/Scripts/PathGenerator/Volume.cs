using UnityEngine;

public interface Volume
{
    Vector3 GetRandomPosition();
    Vector3 GetRandomPosition(float para);
    Vector3 GetRandomPosition(float para1, float para2);

    bool IsInside(Vector3 position);
}