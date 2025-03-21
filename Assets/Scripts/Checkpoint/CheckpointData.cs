// using System;
// using Player;
// using VContainer;
using UnityEngine;

namespace Checkpoint {
    public class CheckpointData : MonoBehaviour {
        // private PlayerScript _playerScript;
        // private Vector2 _playerRespawnPos;
        // private float _playerPivotToGroundDistance;
        [HideInInspector] public LevelCheckpoint[] combatCheckpoints;
        //
        // [Inject] private IAudioManager _audioManager;

        public void Initialize() {
            // _playerScript = ServiceLocator.Get<PlayerScript>();
            // _playerScript.OnRespawn += OnRespawn;
            // var playerCollider = _playerScript.gameObject.GetComponent<Collider2D>();
            // _playerPivotToGroundDistance = playerCollider.bounds.min.y - playerCollider.transform.position.y;
            //
            // foreach (var checkpoint in combatCheckpoints) {
            //     checkpoint.CheckpointReached += ReachedCheckpoint;
            // }
            //
            // ReachedCheckpoint();
        }

        // private void OnDisable()
        // {
        //     _playerScript.OnRespawn -= OnRespawn;
        // }
        //
        // private void ReachedCheckpoint() {
        //     var hit = Physics2D.Raycast(_playerScript.gameObject.transform.position + Vector3.up * 0.1f,
        //         Vector2.down, 10, LayerMask.GetMask("Ground"));
        //     if (hit.collider != null) {
        //         _playerRespawnPos = hit.point;
        //     }
        // }
        //
        // private void OnRespawn() {
        //     // _audioManager.PlayOneShot(PlayerScript.PlayerRespawn_SoundID);
        // }
    }
}