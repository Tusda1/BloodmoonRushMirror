using Player;
using UnityEngine;

public class LadderMiddle : MonoBehaviour {
    #region ---Fields---
    [Header("Debug Logger")]
    [SerializeField] private DebugLoggerSO debugLogger;
    #endregion

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        debugLogger?.Log("Ladder middle entered", this);

        other.GetComponent<PlayerScript>().playerMovement.ladderMiddle = this;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        debugLogger?.Log("Ladder middle exited", this);

        other.GetComponent<PlayerScript>().playerMovement.ladderMiddle = null;
    }
}