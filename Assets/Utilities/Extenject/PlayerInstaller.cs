using Player;
using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller {
    [SerializeField] private PlayerScript playerScript;
    
    public override void InstallBindings() {
        Container.BindInstance(playerScript);
        Container.BindInstance(playerScript.playerMovement);
        Container.BindInstance(playerScript.hitbox);
    }
}