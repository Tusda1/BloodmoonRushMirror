using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainMenuInstaller : LifetimeScope {
    [SerializeField] private Camera mainCamera;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(mainCamera);
    }
}