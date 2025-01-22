using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller {
    [SerializeField] private Camera mainCamera;
    
    public override void InstallBindings() {
        Container.BindInstance(mainCamera).WithId("MainCamera");
        Container.Bind<Transform>().WithId("MainCamera").FromInstance(mainCamera.transform);
    }
}