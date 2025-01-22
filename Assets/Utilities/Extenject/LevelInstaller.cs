using SaveLoadSystem;
using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LevelDataManager levelDataManager;
    
    public override void InstallBindings() {
        Container.BindInstance(mainCamera).WithId("MainCamera");
        Container.Bind<Transform>().WithId("MainCamera").FromInstance(mainCamera.transform);
        Container.BindInstance(levelDataManager);
    }
}