using UnityEditor;
using UnityEngine;

public class BuildPreProcessor : MonoBehaviour
{
#if UNITY_CLOUD_BUILD
    public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
    {
        string buildNumber = manifest.GetValue("buildNumber", "0");
        Debug.LogWarning("Setting build number to " + buildNumber);
        PlayerSettings.iOS.buildNumber = buildNumber;
        Debug.Log("BuildPreProcessor Finished");
        PlayerSettings.iOS.hideHomeButton = true;
    }
#endif
}