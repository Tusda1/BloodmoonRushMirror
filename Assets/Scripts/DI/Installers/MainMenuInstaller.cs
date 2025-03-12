using Unity.Cinemachine;
using UnityEngine;
using VContainer;

public class MainMenuInstaller : AutoInjectInstaller {
    [SerializeField] private Camera mainCamera;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstanceID(mainCamera, LevelInstaller.MainCameraID);
        builder.RegisterInstance(mainCamera.GetComponent<CinemachineBrain>());
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        SetReferenceFromSceneIfNull(ref mainCamera);
    }
#endif
}