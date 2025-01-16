using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;

public static class Builder {
    #region ---Fields---
    private const string ContextMenuPath = "Tools/Builder/";

    #region ***---Steam---***
    private const string SteamContextMenuPath = ContextMenuPath + "Steam/";
    
    private const string SteamBuildProfilePath = "Assets/Settings/Build Profiles/Steam.asset";
    private const string SteamDemoProfilePath = "Assets/Settings/Build Profiles/SteamDemo.asset";

    private const string SteamBuildLocationPath = @"C:\Users\erlen\Documents\steamworks_sdk_161\sdk\tools\ContentBuilder\content\Windows";
    
    private const string SteamBuildBatchFilePath = @"C:\Users\erlen\Documents\steamworks_sdk_161\sdk\tools\ContentBuilder\scripts\run_build_3360780.bat";
    private const string SteamDemoBuildBatchFilePath = @"C:\Users\erlen\Documents\steamworks_sdk_161\sdk\tools\ContentBuilder\scripts\run_build_3468140.bat";

    #endregion
    #endregion
    
    #region---Steam---
    [MenuItem(SteamContextMenuPath + nameof(OpenSteamBuildLocation))]
    public static void OpenSteamBuildLocation() {
        Process.Start(SteamBuildLocationPath);
    }
    
    [MenuItem(SteamContextMenuPath + nameof(UploadSteamBuild))]
    public static void UploadSteamBuild() {
        BuildSteamProfileAtPath(SteamBuildProfilePath);
        
        DeleteDoNotShipFiles(SteamBuildLocationPath);
        
        RunSteamBuildBatchFile();
    }
    
    [MenuItem(SteamContextMenuPath + nameof(UploadSteamDemoBuild))]
    public static void UploadSteamDemoBuild() {
        BuildSteamProfileAtPath(SteamDemoProfilePath);
        
        DeleteDoNotShipFiles(SteamBuildLocationPath);
        
        RunSteamDemoBuildBatchFile();
    }

    private static void RunSteamBuildBatchFile() {
        var process = new Process();
        process.StartInfo.FileName = SteamBuildBatchFilePath;
        process.Start();
    }
    
    private static void RunSteamDemoBuildBatchFile() {
        var process = new Process();
        process.StartInfo.FileName = SteamDemoBuildBatchFilePath;
        process.Start();
    }

    private static void DeleteDoNotShipFiles(string directory) {
        const string doNotShip = "BloodmoonRush_BurstDebugInformation_DoNotShip";
        var doNotShipPath = Path.Combine(directory, doNotShip);
        if (Directory.Exists(doNotShipPath)) {
            Directory.Delete(doNotShipPath, true);
        }
    }

    private static void BuildSteamProfileAtPath(string profilePath) {
        FileUtil.DeleteFileOrDirectory(SteamBuildLocationPath);
        Directory.CreateDirectory(SteamBuildLocationPath);
        
        BuildProfileAtPath(profilePath, SteamBuildLocationPath);
    }
    #endregion

    private static void BuildProfileAtPath(string profilePath, string locationPath) {
        var currentBuildProfile = BuildProfile.GetActiveBuildProfile();
        var buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
        
        var profileOptions = new BuildPlayerWithProfileOptions {
            buildProfile = buildProfile,
            locationPathName = locationPath + "/BloodmoonRush.exe",
            options = BuildOptions.None
        };
        
        BuildPipeline.BuildPlayer(profileOptions);
        BuildProfile.SetActiveBuildProfile(currentBuildProfile);
    }
}