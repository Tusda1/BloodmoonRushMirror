using FMODScripts;
using Player;
using UnityEngine;

public class CheckpointData : MonoBehaviour {
    private PlayerScript _playerScript;
    private Vector2 _playerRespawnPos;
    [HideInInspector] public CombatCheckpoint[] combatCheckpoints;
    
    private static FMODManager FMODManager => FMODManager.Instance;

    public void Initialize() {
        _playerScript = ServiceLocator.Get<PlayerScript>();
        _playerScript.OnRespawn += Respawn;
        foreach (var checkpoint in combatCheckpoints) {
            checkpoint.CheckpointReached += ReachedCheckpoint;
        }

        ReachedCheckpoint();
    }

    private void ReachedCheckpoint() {
        print("Checkpoint reached");
        _playerRespawnPos = _playerScript.gameObject.transform.position;
    }

    private void Respawn() {
        FMODManager.PlaySfx("Player/SpawnPlayer");
        _playerScript.gameObject.transform.position = _playerRespawnPos;
    }
}