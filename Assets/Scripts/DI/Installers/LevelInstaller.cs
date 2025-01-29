using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelInstaller : LifetimeScope {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private LevelDataManager levelDataManager;
    [SerializeField] private SteamSpeedrunTimer steamSpeedrunTimer;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(mainCamera);
        builder.RegisterInstance(cinemachineBrain);
        builder.RegisterInstance(levelDataManager);
        builder.RegisterInstance(steamSpeedrunTimer);
    }
}