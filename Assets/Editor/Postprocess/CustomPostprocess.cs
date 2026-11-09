using System.Collections;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor;
using UnityEngine;
using ProGamer;

public class CustomPostprocess
{
    [PostProcessBuild(9999)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            //Library
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.GetUnityMainTargetGuid();
            proj.AddFrameworkToProject(target, "AssetsLibrary.framework", false);
            //For AppsFlyer
            proj.AddFrameworkToProject(target, "Security.Framework", false);
            proj.AddFrameworkToProject(target, "AdSupport.framework", false);
            proj.AddFrameworkToProject(target, "iAd.framework", false);

            File.WriteAllText(projPath, proj.WriteToString());

            //Plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;

            rootDict.SetString("NSPhotoLibraryAddUsageDescription", GameSetting.NSPhotoLibraryAddUsageDescription);
            rootDict.SetString("NSPhotoLibraryUsageDescription", GameSetting.NSPhotoLibraryUsageDescription);
            //rootDict.SetString("NSCameraUsageDescription", GameSetting.NSCameraUsageDescription);

            PlistElementArray local = rootDict.CreateArray("CFBundleLocalizations");
            local.AddString("en");

            local.AddString("fr");
            local.AddString("de");
            local.AddString("it");
            local.AddString("es");
            local.AddString("pt");
            local.AddString("ja");
            local.AddString("ko");

            local.AddString("zh-Hans");
            local.AddString("zh-Hant");

            local.AddString("pt-BR");
            local.AddString("es-MX");

            local.AddString("ru");


            //#if IPHONEX_CROP
            ////iPhoneX fix
            //string controllerPath = path + "/Classes/UnityAppController.mm";
            //string original = "_window         = [[UIWindow alloc] initWithFrame: [UIScreen mainScreen].bounds];";
            //string replacement = EditorUtils.LoadText("Assets/R/Scripts/PythonTools/Editor/iPhoneXSolution.txt");

            //PythonRunner.Replace(controllerPath, original, replacement);
            //#endif

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}