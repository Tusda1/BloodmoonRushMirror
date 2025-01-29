using Attributes;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Serializers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.DataBinding;
using VContainer;
using VContainer.Unity;

public class PersistentManagersInstaller : LifetimeScope {
    [SerializeField, Required] private SaveLoadSystemManager saveLoadSystemManager;
    [SerializeField, Required] private CameraEffects cameraEffects;
    [SerializeField, Required] private EventSystem eventSystem;
    [SerializeField, Required] private Camera renderTextureCamera;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(saveLoadSystemManager);
        builder.RegisterInstance(cameraEffects);
        builder.RegisterInstance(eventSystem);
        builder.RegisterInstance(renderTextureCamera);

        builder.Register<FileDataService<SaveGameData>>(Lifetime.Singleton);
        builder.Register<JSONSerializer>(Lifetime.Singleton).As<ISerializer>();
    }
}