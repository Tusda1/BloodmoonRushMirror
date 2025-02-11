using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

public class LevelInstaller : AutoInjectInstaller {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private LevelDataManager levelDataManager;
    [SerializeField] private LevelMusicManager levelMusicManager;
    [SerializeField] private string leaderboardID;
    
    public const string MainCameraID = "MainCamera";

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstanceID(mainCamera, MainCameraID);
        builder.RegisterInstance(cinemachineBrain);
        builder.RegisterInstance(levelDataManager);
        builder.RegisterInstance(levelMusicManager);
        builder.RegisterInstance(new SteamSpeedrunTimer(leaderboardID)).As<ISpeedrunTimer>();
    }

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