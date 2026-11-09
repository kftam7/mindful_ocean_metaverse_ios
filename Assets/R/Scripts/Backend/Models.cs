using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class ConfigEntry<T>
    {
        [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionId;

        [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionName;

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("configName", NullValueHandling = NullValueHandling.Ignore)]
        public string ConfigName;

        [JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
        public T Config;

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public string Created;

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated;
    }

    public class CollectionResponse<T>
    {
        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public int Page { get; set; }

        [JsonProperty("perPage", NullValueHandling = NullValueHandling.Ignore)]
        public int PerPage { get; set; }

        [JsonProperty("totalPages", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalPages { get; set; }

        [JsonProperty("totalItems", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalItems { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<T> Items { get; set; }
    }

    public class ServerTimeEntry
    {
        [JsonProperty("year", NullValueHandling = NullValueHandling.Ignore)]
        public int Year;

        [JsonProperty("month", NullValueHandling = NullValueHandling.Ignore)]
        public int Month;

        [JsonProperty("day", NullValueHandling = NullValueHandling.Ignore)]
        public int Day;

        [JsonProperty("hour", NullValueHandling = NullValueHandling.Ignore)]
        public int Hour;

        [JsonProperty("minute", NullValueHandling = NullValueHandling.Ignore)]
        public int Minute;

        [JsonProperty("seconds", NullValueHandling = NullValueHandling.Ignore)]
        public int Seconds;

        [JsonProperty("milliSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public int MilliSeconds;

        [JsonProperty("dateTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime DateTime;

        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        public string Date;

        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public string Time;

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeZone;

        [JsonProperty("dayOfWeek", NullValueHandling = NullValueHandling.Ignore)]
        public string DayOfWeek;

        [JsonProperty("dstActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool DstActive;
    }
}