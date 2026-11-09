using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class BuildPostProcessing
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS)
        {
            return;
        }

        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string frameworkGuid = proj.GetUnityFrameworkTargetGuid();

        // Module map
        proj.AddBuildProperty(frameworkGuid, "DEFINES_MODULE", "YES");

        var moduleFile = path + "/UnityFramework/UnityFramework.modulemap";
        if (!File.Exists(moduleFile))
        {
            FileUtil.CopyFileOrDirectory("Assets/Plugins/iOS/HandDetector/Native/UnityFramework.modulemap", moduleFile);
            proj.AddFile(moduleFile, "UnityFramework/UnityFramework.modulemap");
            proj.AddBuildProperty(frameworkGuid, "MODULEMAP_FILE", "$(SRCROOT)/UnityFramework/UnityFramework.modulemap");
        }

        // Headers
        string unityInterfaceGuid = proj.FindFileGuidByProjectPath("Classes/Unity/UnityInterface.h");
        proj.AddPublicHeaderToBuild(frameworkGuid, unityInterfaceGuid);

        string unityForwardDeclsGuid = proj.FindFileGuidByProjectPath("Classes/Unity/UnityForwardDecls.h");
        proj.AddPublicHeaderToBuild(frameworkGuid, unityForwardDeclsGuid);

        string unityRenderingGuid = proj.FindFileGuidByProjectPath("Classes/Unity/UnityRendering.h");
        proj.AddPublicHeaderToBuild(frameworkGuid, unityRenderingGuid);

        string unitySharedDeclsGuid = proj.FindFileGuidByProjectPath("Classes/Unity/UnitySharedDecls.h");
        proj.AddPublicHeaderToBuild(frameworkGuid, unitySharedDeclsGuid);

        proj.SetBuildProperty(frameworkGuid, "ENABLE_BITCODE", "NO");

        proj.WriteToFile(projPath);

        // Get plist
        var plistPath = path + "/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        var rootDict = plist.root;

        var buildKey2 = "ITSAppUsesNonExemptEncryption";
        rootDict.SetBoolean(buildKey2, false);

        PlistElementDict allowsDict = plist.root.CreateDict("NSAppTransportSecurity");

        allowsDict.SetBoolean("NSAllowsArbitraryLoads", true);

        PlistElementDict exceptionsDict = allowsDict.CreateDict("NSExceptionDomains");

        PlistElementDict domainDict = exceptionsDict.CreateDict("16.162.241.30");
        domainDict.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
        domainDict.SetBoolean("NSIncludesSubdomains", true);

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}