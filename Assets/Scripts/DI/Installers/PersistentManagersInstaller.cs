using Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class PersistentManagersInstaller : AutoInjectInstaller {
    // [SerializeField, Required] private SaveLoadSystemManager saveLoadSystemManager;
    [SerializeField, Required] private CameraEffects cameraEffects;
    [SerializeField, Required] private EventSystem eventSystem;
    [SerializeField, Required] private Camera renderTextureCamera;

    public const string RenderTextureCameraID = "RenderTextureCamera";

    protected override void Configure(IContainerBuilder builder) {
        //builder.RegisterInstance(saveLoadSystemManager);
        builder.RegisterInstance(cameraEffects);
        builder.RegisterInstance(eventSystem);
        builder.RegisterInstanceID(renderTextureCamera, RenderTextureCameraID);
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        // SetReferenceFromSceneIfNull(ref saveLoadSystemManager);
        SetReferenceFromSceneIfNull(ref cameraEffects);
        SetReferenceFromSceneIfNull(ref eventSystem);
        SetReferenceFromSceneIfNull(ref renderTextureCamera);
    }
#endif
}