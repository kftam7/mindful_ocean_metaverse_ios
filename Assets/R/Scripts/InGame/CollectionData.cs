using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class CollectionData
{
    public string Theme { get; set; }
    public string ThemeName { get; set; }
    public string Preview { get; set; }
    public float PreviewSize { get; set; }
    public string Ground { get; set; }
    public string ARGround { get; set; }
    public List<string> DefaultCreature { get; set; }
    public List<string> DefaultSkin { get; set; }
    public string Icon { get; set; }
    public string ARIcon { get; set; }
    public string StyleIcon { get; set; }
    public string StyleIconTexture { get; set; }
    public string BookPageTexture { get; set; }
    public float InitScale { get; set; } // only for ar mode

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    //This class is designed for store data. if need to modify it, use clone for duplicate it
    public CollectionData Clone()
    {
        return JsonConvert.DeserializeObject<CollectionData>(this.ToString());
    }
}