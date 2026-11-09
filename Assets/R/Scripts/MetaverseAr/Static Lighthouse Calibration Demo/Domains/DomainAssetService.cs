using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Domains;
using Matterless;
using Auki.Navigation;
using Newtonsoft.Json;


namespace Auki
{
    public class DomainAssetService : ApiService
    {
        readonly JsonSerializerSettings m_Settings = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture
        };
        
        private readonly IRestService m_RestService;
        
        public bool IsCalling => m_RestService.IsCalling;

        public DomainAssetService(IRestService restService)
        {
            m_RestService = restService;
        }

        public void GetNavMesh(string domainId, AssetType type, Action<SerializedNavData> callback, Action<RestService.ErrorResponse> onError)
        {
            if (type != AssetType.baked && type != AssetType.navmesh)
                throw new InvalidOperationException($"asset type must be {AssetType.baked} or {AssetType.navmesh}.");

            Dictionary<string, object> requestBody = new()
            {
                { "app_id", appId },
                { "domain_id", domainId },
                { "tags", new Dictionary<string, object> { { "$in", new[] { type.ToString() } } }}
            };  
            
            m_RestService.UnsecurePostJson($"{url}/arbitrary/query", JsonConvert.SerializeObject(requestBody),result =>
            {
                List<AssetData> list = JsonConvert.DeserializeObject<List<AssetData>>(result, m_Settings);
                if (list == null)
                {
                    callback(new SerializedNavData());
                    return;
                }

                AssetData assetData = list.FirstOrDefault();
                callback(new SerializedNavData().FromAssetData(assetData));
            },
            onError);
        }
    }

    public enum AssetType
    {
        navmesh,
        baked
    }
}