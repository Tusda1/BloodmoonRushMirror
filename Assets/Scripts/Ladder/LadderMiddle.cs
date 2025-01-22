using Player;
using UnityEngine;

public class LadderMiddle : MonoBehaviour {
    #region ---Fields---
    [Header("Debug Logger")]
    [SerializeField] private DebugLoggerSO debugLogger;
    #endregion

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.TryGetComponent<PlayerScript>(out var playerScript)) return;

        debugLogger?.Log("Ladder middle entered", this);

        playerScript.playerMovement.PlayerLadderLogic.LadderMiddle = this;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.TryGetComponent<PlayerScript>(out var playerScript)) return;

        debugLogger?.Log("Ladder middle exited", this);

        playerScript.playerMovement.PlayerLadderLogic.LadderMiddle = null;
    }
}