using System;
using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
#if STEAM_BUILD
using Steamworks;
#endif

public class LevelInstaller : AutoInjectInstaller {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private LevelDataManager levelDataManager;
    [SerializeField] private LevelMusicManager levelMusicManager;
    [SerializeField] private string leaderboardID;
    [SerializeField] private string steamStatus = "#status_";
    
    public const string MainCameraID = "MainCamera";

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstanceID(mainCamera, MainCameraID);
        builder.RegisterInstance(cinemachineBrain);
        builder.RegisterInstance(levelDataManager);
        builder.RegisterInstance(levelMusicManager);
        
        // --- SpeedrunTimer --- \\
        builder.RegisterInstanceID(leaderboardID, "LeaderboardID");
        builder.Register<SteamSpeedrunTimer>(Lifetime.Scoped)
            .As<ISpeedrunTimer>();
    }
#if STEAM_BUILD
    private void Start() {
        SteamFriends.SetRichPresence("status", steamStatus);
    }
#endif
#if UNITY_EDITOR
    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        SetReferenceFromSceneIfNull(ref mainCamera);
        SetReferenceFromSceneIfNull(ref cinemachineBrain);
        SetReferenceFromSceneIfNull(ref levelDataManager);
        SetReferenceFromSceneIfNull(ref levelMusicManager);
    }
#endif
}