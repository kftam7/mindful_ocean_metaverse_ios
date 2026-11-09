using System.IO;
using System.Text;
using Auki.Navigation;
using Dummiesman;
using UnityEngine;

namespace Matterless
{
    public class SerializedNavData
    {
        private static uint _Id = 0;
        public uint LocalId;
        public string NavMeshObj;
        public string Guid;
        public ulong LastUpdateTIme;
        
        public SerializedNavData FromAssetData(AssetData assetData)
        {
            LocalId = ++_Id;
            Guid = assetData._id;
            LastUpdateTIme = assetData.updated_at;
            NavMeshObj = (string)assetData.data?["navmesh"];
            return this;
        }

        public GameObject DeserializeNavMesh()
        {
            if (string.IsNullOrWhiteSpace(NavMeshObj))
                return null;
            
            byte[] bytes = GetBytes(NavMeshObj);
            MemoryStream stream = new(bytes);
            GameObject navMeshGeometry = new OBJLoader().Load(stream);
            return navMeshGeometry;
        }
        
        /// <summary>
        /// Encode a UTF8 string to a bytes array.
        /// </summary>
        /// <returns>The bytes array of a UTF8 string.</returns>
        /// <param name="str">String.</param>
        private static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}