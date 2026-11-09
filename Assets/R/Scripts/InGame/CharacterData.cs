public class CharacterData
{
    public string ModelName { get; set; }
    public string DisplayName { get; set; }
    public string PrefabPath { get; set; }
    public string DisplayAnimation { get; set; }
    public bool IsDefault { get; set; }
    public int Price { get; set; }
    public string FishTankModelName { get; set; }
    public int DisplayAmount { get; set; }
    public float AquariumSize { get; set; }
    public float RealSizeRef { get; set; } // for magic object behaviours
    public float AquariumSpeed { get; set; }
    public float ListSize { get; set; }
    public float ListYOffset { get; set; } // 0-1
    public float ListAnimPosition { get; set; } // 0-1
    public float PreviewSize { get; set; }
    public float PreviewYOffset { get; set; }
    public float PreviewYAngle { get; set; }
}