using Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerInstaller : LifetimeScope {
    [SerializeField] private PlayerScript playerScript;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(playerScript);
        builder.RegisterInstance(playerScript.playerMovement);
        builder.RegisterInstance(playerScript.hitbox);
    }
}