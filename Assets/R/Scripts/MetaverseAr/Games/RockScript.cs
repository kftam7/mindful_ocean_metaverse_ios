using UnityEngine;

public class RockScript : MonoBehaviour
{
    [SerializeField] private RandomMesh randomMesh = new();

    public int MeshesLength => randomMesh.MeshesLength;
    
    public void Setup(int index)
    {
        randomMesh.Initialize(out var m, index);
    }
}