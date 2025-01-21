using Attributes;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Serializers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.DataBinding;
using Zenject;

public class PersistentManagersInstaller : MonoInstaller {
    [SerializeField, Required] private SaveLoadSystemManager saveLoadSystemManager;
    [SerializeField, Required] private CameraEffects cameraEffects;
    [SerializeField, Required] private EventSystem eventSystem;
    [SerializeField, Required] private Camera renderTextureCamera;
    
    public override void InstallBindings() {
        Container.BindInstance(saveLoadSystemManager);
        Container.BindInstance(cameraEffects);
        Container.BindInstance(eventSystem);
        Container.BindInstance(renderTextureCamera);

        Container.Bind<FileDataService<SaveGameData>>().AsSingle();
        Container.Bind<ISerializer>().FromInstance(new JSONSerializer());
    }
}