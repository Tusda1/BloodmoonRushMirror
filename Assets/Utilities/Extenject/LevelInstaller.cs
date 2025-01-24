using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private LevelDataManager levelDataManager;
    [SerializeField] private SteamSpeedrunTimer steamSpeedrunTimer;
    
    public override void InstallBindings() {
        Container.BindFromHierarchyIfNull(mainCamera, "MainCamera");
        Container.BindFromHierarchyIfNull(cinemachineBrain);
        Container.BindFromHierarchyIfNull(levelDataManager);
        Container.BindFromHierarchyIfNull(steamSpeedrunTimer);
    }
}