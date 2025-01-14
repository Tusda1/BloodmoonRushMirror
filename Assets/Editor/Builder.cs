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
    private const string SteamDevelopmentProfilePath = "Assets/Settings/Build Profiles/SteamDevelopment.asset";
    private const string SteamBuildLocationPath = @"C:\Users\erlen\Documents\steamworks_sdk_161\sdk\tools\ContentBuilder\content\Windows";
    private const string SteamBuildBatchFilePath = @"C:\Users\erlen\Documents\steamworks_sdk_161\sdk\tools\ContentBuilder\scripts\run_build_3360780.bat";
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
        
        DeleteDoNotShipFiles();
        
        RunSteamBuildBatchFile();
    }
    
    [MenuItem(SteamContextMenuPath + nameof(UploadSteamDevelopmentBuild))]
    public static void UploadSteamDevelopmentBuild() {
        BuildSteamProfileAtPath(SteamDevelopmentProfilePath);
        
        RunSteamBuildBatchFile();
    }

    private static void RunSteamBuildBatchFile() {
        var process = new Process();
        process.StartInfo.FileName = SteamBuildBatchFilePath;
        process.Start();
    }

    private static void DeleteDoNotShipFiles() {
        const string doNotShip = "BloodmoonRush_BurstDebugInformation_DoNotShip";
        if (Directory.Exists(doNotShip)) {
            Directory.Delete(doNotShip, true);
        }
    }
    #endregion

    private static void BuildSteamProfileAtPath(string profilePath) {
        FileUtil.DeleteFileOrDirectory(SteamBuildLocationPath);
        Directory.CreateDirectory(SteamBuildLocationPath);
        
        BuildProfileAtPath(profilePath, SteamBuildLocationPath);
    }

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