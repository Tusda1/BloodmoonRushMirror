using Player;
using UnityEngine;

public class LadderTop : MonoBehaviour {
    #region ---Fields---
    public Transform ladderTopPosition;
    public Transform ladderTopBottom;
    
    public LadderMiddle ladderMiddle;
    
    [Header("Debug Logger")]
    [SerializeField] private DebugLoggerSO debugLogger;
    #endregion

    private void OnCollisionEnter2D(Collision2D other) {
        if (!other.transform.CompareTag("Player")) return;
        
        debugLogger?.Log("Player collided with ladder top", this);
        
        var player = other.gameObject.GetComponent<PlayerScript>();
        player.playerMovement.ladderTop = this;
    }
    
    private void OnCollisionExit2D(Collision2D other) {
        if (!other.transform.CompareTag("Player")) return;
        
        debugLogger?.Log("Player stopped colliding with ladder top", this);
        
        var player = other.gameObject.GetComponent<PlayerScript>();
        player.playerMovement.ladderTop = null;
    }
}