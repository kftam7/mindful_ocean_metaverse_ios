using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class RandomMesh
{
    [SerializeField] private MeshRenderer[] meshes;

    public int MeshesLength => meshes.Length;

    public void Initialize(out Material material, int preDefineId = -1)
    {
        var id = preDefineId < 0 ? Random.Range(0, meshes.Length) : preDefineId;

        foreach (var item in meshes)
        {
            item.gameObject.SetActive(false);
        }

        meshes[id].gameObject.SetActive(true);

        material = meshes[id].gameObject.GetComponent<MeshRenderer>().material;
    }
}