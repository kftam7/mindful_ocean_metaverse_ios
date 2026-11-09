using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dds.Model
{
    public class Domain
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("redirect_url", NullValueHandling = NullValueHandling.Ignore)]
        public object RedirectUrl;

        [JsonProperty("organization_id", NullValueHandling = NullValueHandling.Ignore)]
        public string OrganizationId;

        [JsonProperty("domain_server_id", NullValueHandling = NullValueHandling.Ignore)]
        public string DomainServerId;

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedAt;

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime UpdatedAt;

        [JsonProperty("claimed_by_kazm_id", NullValueHandling = NullValueHandling.Ignore)]
        public object ClaimedByKazmId;

        [JsonProperty("owner_wallet_address", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerWalletAddress;

        [JsonProperty("domain_server", NullValueHandling = NullValueHandling.Ignore)]
        public DomainServer DomainServer;
    }

    public class DomainServer
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("organization_id", NullValueHandling = NullValueHandling.Ignore)]
        public string OrganizationId;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url;

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version;

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status;

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedAt;

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime UpdatedAt;

        [JsonProperty("redirect_url", NullValueHandling = NullValueHandling.Ignore)]
        public object RedirectUrl;

        [JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
        public string Mode;

        [JsonProperty("variants", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Variants;

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description;

        [JsonProperty("staking_status", NullValueHandling = NullValueHandling.Ignore)]
        public string StakingStatus;

        [JsonProperty("ip", NullValueHandling = NullValueHandling.Ignore)]
        public string Ip;

        [JsonProperty("latitude", NullValueHandling = NullValueHandling.Ignore)]
        public double Latitude;

        [JsonProperty("longitude", NullValueHandling = NullValueHandling.Ignore)]
        public double Longitude;

        [JsonProperty("asn", NullValueHandling = NullValueHandling.Ignore)]
        public int Asn;

        [JsonProperty("h3_cell_index", NullValueHandling = NullValueHandling.Ignore)]
        public string H3CellIndex;

        [JsonProperty("cloud_region", NullValueHandling = NullValueHandling.Ignore)]
        public string CloudRegion;
    }

    public class SharedSessionInfo
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("app_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AppId;

        [JsonProperty("domain_id", NullValueHandling = NullValueHandling.Ignore)]
        public string DomainId;

        [JsonProperty("session_guid", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionGuid;

        [JsonProperty("threshold", NullValueHandling = NullValueHandling.Ignore)]
        public int Threshold;

        [JsonProperty("last_active_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastActiveAt;

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedAt;

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime UpdatedAt;

        [JsonProperty("domain", NullValueHandling = NullValueHandling.Ignore)]
        public Domain Domain;

        public string GetSessionId()
        {
            var colonId = SessionGuid.IndexOf(':');
            return SessionGuid[..colonId];
        }
    }

    public class SharedSessionRequest
    {
        [JsonProperty("app_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AppId;

        [JsonProperty("domain_id", NullValueHandling = NullValueHandling.Ignore)]
        public string DomainId;

        [JsonProperty("join_session", NullValueHandling = NullValueHandling.Ignore)]
        public bool JoinSession;

        [JsonProperty("only_same_app", NullValueHandling = NullValueHandling.Ignore)]
        public bool OnlySameApp;

        [JsonProperty("session_guid", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionGuid;

        [JsonProperty("threshold", NullValueHandling = NullValueHandling.Ignore)]
        public int Threshold;

        [JsonProperty("with_domain", NullValueHandling = NullValueHandling.Ignore)]
        public bool WithDomain;
    }
}