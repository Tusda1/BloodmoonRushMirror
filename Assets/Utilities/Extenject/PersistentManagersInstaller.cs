using Attributes;
using SaveLoadSystem;
using UnityEngine;
using UnityEngine.EventSystems;
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
    }
}