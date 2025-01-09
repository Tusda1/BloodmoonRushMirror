using Player;
using UnityEngine;

public class PlatformCheckpoint : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private GameObject respawnPoint;
    #endregion

    private void Start() {
        respawnPoint.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        
        if (other.TryGetComponent(out GravityObjectPlayerMovement playerMovement)) {
            playerMovement.lastPlatformCheckpoint = respawnPoint.transform.position;
        }
    }
}