using Attributes;
using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller {
    [SerializeField, Required] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField, Required] private LevelDataManager levelDataManager;
    
    public override void InstallBindings() {
        Container.BindFromHierarchyIfNull(mainCamera, "MainCamera");
        Container.BindFromHierarchyIfNull(cinemachineBrain);
        Container.BindFromHierarchyIfNull(levelDataManager);
    }
}