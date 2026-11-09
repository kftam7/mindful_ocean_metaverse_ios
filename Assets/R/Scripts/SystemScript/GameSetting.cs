using Network;
using UnityEngine;

public class GameSetting : Network.Singleton<GameSetting>
{
    public int ProdVersionId = 10;
    public int DevVersionId = 10;
    public int VersionId => NetworkSettings.Instance.Environment == NetworkEnvironmentEnum.Product ? ProdVersionId : DevVersionId;
    public string CustomerSupport = "";
    public string PrivacyPolicy = "";
    public string GameWebSite = "";
    public string GameWebSiteFaq = "";
    public string AppStoreUrl = "";
    public string TutorialHatcheryUrl = "";
    public string TutorialMetaverseUrl = "";
    public string TutorialFaqUrl = "";
    public string PrivacyPolicyUrl = "";

    public const string GameName = "Mindful Ocean";
    public const string DefaultFish = "usr_TangFish_Cartoon_default.jpg";
    public const string AndroidPackageName = "com.animocabrands.google.aroceancoloring";

    public const string DefaultModel = "Dolphin";
    public const int HPOriginLimit = 100;
    public const int WallDamage = 10;
    public const string WallHitEffect = "Prefabs/Effects/hit_eff";
    public const int BaseFoodBonus = 10;
    public static readonly float[] FoodBonus = { 1f, 1.1f, 1.2f };

    public const string ModelPrefabPath = "Models/Tanks/";
    public const int LeaderboardLimitation = 30;

    //Drawing
    public const float TreatAsAlphaRange = 0.75f;

    public const float AlphaMultiplier = 2f;

    //original 5
    public static int DrawingHistoryForMobileDevices => SystemInfo.systemMemorySize >= 2048 ? 20 : 19;
    public const int DrawingHistoryForDesktopDevices = 20; //original 10

    public const string DefaultAquariumTheme = "Mandala";
    public const string DefaultAquariumSaveKey = "DEFAULT_";
    public const string HatcheryAquariumId = "HatcheryAquarium";
    public const string HatcheryAquariumName = "Hatchery"; // name in AquariumData.
    public const string HatcheryAquariumSnapshotFileName = "hatchery_aquarium.jpg";
    public const string HatcheryAquariumTheme = "Mandala3";
    public const string MetaverseAquariumId = "MetaverseAquarium";
    public const string MetaverseAquariumName = "Metaverse";
    public const string MetaverseAquariumTheme = "Mandala";
    public const string MetaverseAquariumSnapshotFileName = "metaverse_aquarium.jpg";

    public const int MaxFishNumberInAquarium = 20;

    #region Permission

    public const string CameraPermission = "android.permission.CAMERA";
    public const string WriteStoragePermission = "android.permission.WRITE_EXTERNAL_STORAGE";

    public const string NSPhotoLibraryAddUsageDescription = "Mindful Ocean needs to access your photo album in order to save coloring pages";
    public const string NSPhotoLibraryUsageDescription = "Mindful Ocean needs to access your storage in order to save coloring pages";
    public const string NSCameraUsageDescription = "Mindful Ocean needs to access your camera to use Augmented Reality";

    #endregion

    #region Configuration

    public static string IAPConfiguration => LoadSetting("IAPConfiguration");
    public static string MaterialConfiguration => LoadSetting("MaterialConfiguration");
    public static string BeginnerTutorial => LoadSetting("BeginnerTutorial");
    public static string PrinterTutorial => LoadSetting("PrinterTutorial");
    public static string CollectionConfiguration => LoadSetting("CollectionConfiguration");
    public static string ImageTargetConfiguration => LoadSetting("ImageTargetConfiguration");
    public static string InGameObjectConfiguration => LoadSetting("InGameObjectConfiguration");
    public static string CharacterConfiguration => LoadSetting("CharacterConfiguration");
    public static string TaskConfiguration => LoadSetting("TaskConfiguration");
    public static string QuestionConfiguration => LoadSetting("QuestionConfiguration");
    public static string EbookConfiguration => LoadSetting("EbookConfiguration");
    public const string SettingPath = "GameSetting/";

    public static string LoadSetting(string setting)
    {
        TextAsset myData = Resources.Load(SettingPath + setting) as TextAsset;
        return myData.text;
    }

    #endregion
}