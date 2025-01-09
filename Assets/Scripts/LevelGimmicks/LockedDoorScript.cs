using Player;
using UnityEngine;

public class LockedDoorScript : MonoBehaviour, IPlayerInteractable {
    #region ---Fields---
    [SerializeField] private BoxCollider2D openCollider;
    [SerializeField] private int openKeyAmount;
    [SerializeField] private float destructionTime;
    private bool _isOpened;
    #endregion
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (_isOpened) return;
        if (other.CompareTag("Player"))
            if (other.TryGetComponent<PlayerScript>(out var playerScript)) {
                Interact(playerScript);
            }
    }

    public void Interact(PlayerScript playerScript) {
        if (playerScript.keyObjects.Count < openKeyAmount) return;
        playerScript.UseKey(transform, openKeyAmount, destructionTime);
                
        OpenDoor();
    }

    public void OpenDoor() {
        _isOpened = true;
        Destroy(gameObject, destructionTime);
    }
}
