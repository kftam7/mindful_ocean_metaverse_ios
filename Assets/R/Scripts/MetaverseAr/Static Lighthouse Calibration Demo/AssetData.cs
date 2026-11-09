using System.Collections.Generic;

namespace Auki.Navigation
{
    [System.Serializable]
    public struct AssetData
    {
        public string _id;
        public Dictionary<string, object> data;
        public ulong updated_at;
    }
}