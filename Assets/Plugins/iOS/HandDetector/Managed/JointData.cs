using UnityEngine;

public class JointData
{
    public Vector2 screenPos;
    public Vector2 texturePos;
    public JointName name;
    public float distance;
    public bool isVisible;
    public float confidence;

    public Vector3 GetWorldPosition(Camera camera, float distance)
    {
        return camera.ScreenToWorldPoint(new Vector3(screenPos.x * Screen.width, screenPos.y * Screen.height, distance));
    }
}