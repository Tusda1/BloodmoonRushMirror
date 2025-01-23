using Attributes;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Serializers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Utilities.DataBinding;
using Zenject;

public class PersistentManagersInstaller : MonoInstaller {
    [SerializeField] private SaveLoadSystemManager saveLoadSystemManager;
    [SerializeField] private CameraEffects cameraEffects;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField, Required] private Camera renderTextureCamera;

    public override void InstallBindings() {
        Assert.IsNotNull(renderTextureCamera);
        
        Container.BindFromHierarchyIfNull(saveLoadSystemManager);
        Container.BindFromHierarchyIfNull(cameraEffects);
        Container.BindFromHierarchyIfNull(eventSystem);
        Container.BindInstance(renderTextureCamera);

        Container.Bind<FileDataService<SaveGameData>>().AsSingle();
        Container.Bind<ISerializer>().FromInstance(new JSONSerializer()).AsSingle().Lazy();
    }
}