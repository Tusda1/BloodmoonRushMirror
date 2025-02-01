using Player;
using UnityEngine;
using VContainer;

public class PlayerInstaller : AutoInjectInstaller {
    [SerializeField] private PlayerScript playerScript;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(playerScript);
        builder.RegisterInstance(playerScript.playerMovement);
        builder.RegisterInstance(playerScript.hitbox);
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        SetReferenceFromSceneIfNull(ref playerScript);
    }
#endif
}