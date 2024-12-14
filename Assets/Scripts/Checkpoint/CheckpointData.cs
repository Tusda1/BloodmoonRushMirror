using FMODScripts;
using Player;
using UnityEngine;

public class CheckpointData : MonoBehaviour {
    private PlayerScript _playerScript;
    private Vector2 _playerRespawnPos;
    private float _playerPivotToGroundDistance;
    [HideInInspector] public LevelCheckpoint[] combatCheckpoints;

    public void Initialize() {
        _playerScript = ServiceLocator.Get<PlayerScript>();
        _playerScript.OnRespawn += Respawn;
        var playerCollider = _playerScript.gameObject.GetComponent<Collider2D>();
        _playerPivotToGroundDistance = playerCollider.bounds.min.y - playerCollider.transform.position.y;
        
        foreach (var checkpoint in combatCheckpoints) {
            checkpoint.CheckpointReached += ReachedCheckpoint;
        }

        ReachedCheckpoint();
    }

    private void ReachedCheckpoint() {
        print("Checkpoint reached");
        
        // Set player respawn position to ground level of current player position
        RaycastHit2D hit = Physics2D.Raycast(_playerScript.gameObject.transform.position + Vector3.up * 0.1f, Vector2.down, 10, LayerMask.GetMask("Ground"));
        if (hit.collider != null) {
            _playerRespawnPos = hit.point + Vector2.up * _playerPivotToGroundDistance;
        }
    }

    private void Respawn() {
        FMODManager.RaiseSFXEvent("Player/SpawnPlayer");
        _playerScript.gameObject.transform.position = _playerRespawnPos;
    }
}