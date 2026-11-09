using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

//Save data
[SerializeField]
public class AquariumData
{
    public string AquariumId { get; set; }
    public string AquariumName { get; set; }
    public string GroundSkin { get; set; }
    public string SnapShotPath { get; set; }
    public string NewMember { get; set; }
    public List<string> DefaultCreature { get; set; }
    public List<string> DefaultSkin { get; set; }
    public string Theme { get; set; }
    public int Index { get; set; }

    [Newtonsoft.Json.JsonIgnoreAttribute]
    private CollectionData themeData;

    public CollectionData Data
    {
        get
        {
            if (themeData != null) return themeData;

            if (!string.IsNullOrEmpty(Theme) && DataCenter.Instance.CollectionDataDic.TryGetValue(this.Theme, out themeData))
            {
                Debug.Log("Get data");
                return themeData;
            }

            Debug.Log("Cannot find theme data.");
            return null;
        }
    }

    public AquariumData(string id, string name, CollectionData data, int index)
    {
        this.AquariumId = id;
        this.AquariumName = name;
        this.DefaultCreature = new List<string>(data.DefaultCreature);
        this.DefaultSkin = new List<string>(data.DefaultSkin);
        this.Theme = data.Theme;
        this.Index = index;
        this.themeData = DataCenter.Instance.CollectionDataDic[data.Theme]; //Ensure get update data
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}