using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Models
{
    public class PlayerData
    {
        public uint participantId;
        public string appleUserId;
        public string appleName;
        public string appleEmail;

        public override string ToString()
        {
            return $"id:{participantId} userId: {appleUserId} name: {appleName} email: {appleEmail}";
        }
    }

    public class ScoreSummary
    {
        public Dictionary<uint, int> playerScores;
        public Dictionary<uint, bool> playersLeftEarly;
    }

    public struct BottlePayload
    {
        public SPose bottlePose;
        public bool spawnProps;
        public SPose weedPose;
        public SPose rockPose;
        public int rockMeshId;
    }

    public class BottleCollectResponse
    {
        [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionId;

        [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionName;

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username;

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email;

        [JsonProperty("collected", NullValueHandling = NullValueHandling.Ignore)]
        public int Collected;

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public string Created;

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated;
    }

    public class BottleCollectedPayload
    {
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username;
        
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email;

        [JsonProperty("collected", NullValueHandling = NullValueHandling.Ignore)]
        public int Collected;
    }
}