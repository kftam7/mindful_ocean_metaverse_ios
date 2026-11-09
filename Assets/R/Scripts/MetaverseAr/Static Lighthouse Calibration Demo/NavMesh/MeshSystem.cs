using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace Matterless.ECS
{
    public class MeshSystem
    {
        private readonly NavMeshSurface m_NavMeshSurface;
        private readonly DomainHelper m_DomainHelper;
        private readonly Material m_NavMeshMaterial;

        public MeshSystem(
            NavMeshSurface navMeshSurface,
            DomainHelper domainHelper,
            Material navMeshMaterial)
        {
            m_NavMeshSurface = navMeshSurface;
            m_DomainHelper = domainHelper;
            m_NavMeshMaterial = navMeshMaterial;
        }

        private readonly Dictionary<Guid, NavMeshModifier> m_Meshes = new();
        private SerializedNavData m_SerializedNavData;
        private SerializedNavData m_SerializedBakedData;
        public IEnumerable<NavMeshModifier> GetMeshes => m_Meshes.Values;

        public event Action MeshesChanged;

        public void Rebuild()
        {
            m_NavMeshSurface.BuildNavMesh();
            
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            
            MeshesChanged?.Invoke();
            Debug.Log($"Rebuilt Navmesh has {m_NavMeshSurface.transform.childCount} children.");
        }

        public void Load(SerializedNavData navData, SerializedNavData bakedData)
        {
            m_SerializedNavData = navData;
            m_SerializedBakedData = bakedData;
            GameObject navMeshGeometry = m_SerializedNavData.DeserializeNavMesh();
            
            if (navMeshGeometry == null)
                return;
            
            List<Transform> list = navMeshGeometry.transform.Cast<Transform>().ToList();
            foreach (Transform child in list)
            {
                child.SetParent(m_NavMeshSurface.transform, false);

                NavMeshModifier navMeshModifier = child.AddComponent<NavMeshModifier>();
                navMeshModifier.overrideArea = true;
                navMeshModifier.area = 0;

                MeshFilter m_BaseShapeMeshFilter = child.GetComponent<MeshFilter>();
                Mesh m_BaseShapeMesh = m_BaseShapeMeshFilter.sharedMesh;
                
                MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = m_NavMeshMaterial;
                
                m_BaseShapeMesh.RecalculateNormals();
            
                string[] splitName = child.gameObject.name.Split(char.Parse(":"));
                Guid guid = new Guid(splitName[0]);
                m_Meshes[guid] = navMeshModifier;
            }

            Object.Destroy(navMeshGeometry);

            Rebuild();
        }
    }
}