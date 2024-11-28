using System;
using Player;
using PlayerLoopTimer;
using UnityEngine;

public class LockedDoorScript : MonoBehaviour {
    #region ---Fields---

    [SerializeField] private BoxCollider2D openCollider;
    [SerializeField] private int openKeyAmount;
    [SerializeField] private float destructionTime;
    private bool _isOpened = false;

    #endregion
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (_isOpened) return;
        if (other.CompareTag("Player"))
            if (other.TryGetComponent<PlayerScript>(out var playerScript)) {
                if (playerScript.keyObjects.Count < openKeyAmount) return;
                _isOpened = true;
                playerScript.UseKey(transform, openKeyAmount, destructionTime);
                Destroy(gameObject, destructionTime);
            }
    }
}
