using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class PreloadSigningAlias
{
    static PreloadSigningAlias()
    {
        Preload();
    }

    public static void Preload()
    {
        if (PlayerSettings.Android.keystorePass.Length == 0)
        {
            Debug.Log("Auto-filled keystore password.");
            PlayerSettings.Android.keystorePass = "OhNaiTh1";
            PlayerSettings.Android.keyaliasName = "animoca";
            PlayerSettings.Android.keyaliasPass = "OhNaiTh1";
        }

        //Unity use individul applicationIdentifier, no more use this
        //string platform = "itunes";
        //if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS){
        //	platform = "itunes";
        //}
        //else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android){
        //	platform = "google";
        //}

        //string[] split = PlayerSettings.applicationIdentifier.Split(new string[]{"itunes","google"},System.StringSplitOptions.None);
        //PlayerSettings.applicationIdentifier = split[0] + platform + split[1];
    }
}