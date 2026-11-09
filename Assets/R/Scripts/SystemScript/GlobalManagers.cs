using UnityEngine;

public class GlobalManagers : Network.Singleton<GlobalManagers>
{
    private void Awake()
    {
        var objects = FindObjectsOfType<GlobalManagers>();
        if (objects.Length > 1)
        {
            Debug.Log("GlobalManager destroyed");
            DestroyImmediate(gameObject);
        }
    }
}