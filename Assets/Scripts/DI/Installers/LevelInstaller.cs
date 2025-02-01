using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

public class LevelInstaller : AutoInjectInstaller {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private LevelDataManager levelDataManager;
    [SerializeField] private string leaderboardID;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(mainCamera);
        builder.RegisterInstance(cinemachineBrain);
        builder.RegisterInstance(levelDataManager);

        builder.RegisterInstance(new SteamSpeedrunTimer(leaderboardID)).As<ISpeedrunTimer>();
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        SetReferenceFromSceneIfNull(ref mainCamera);
        SetReferenceFromSceneIfNull(ref cinemachineBrain);
        SetReferenceFromSceneIfNull(ref levelDataManager);
    }
#endif
}