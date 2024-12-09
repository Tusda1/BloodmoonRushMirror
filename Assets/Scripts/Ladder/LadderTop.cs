using Player;
using UnityEngine;

public class LadderTop : MonoBehaviour, ILadder {

    #region ---Fields---
    public Transform ladderTopPosition;
    public BoxCollider2D boxCollider2D;
    
    private static readonly int ClimbToTop = Animator.StringToHash("LadderExitAtTopTrigger");
    #endregion

    public void SetColliderTrigger(bool value) {
        boxCollider2D.isTrigger = value;
    }

    public void PlayerClimbToTopOfLadder(PlayerScript player) {
        player.transform.position = ladderTopPosition.position;
        player.animator.SetTrigger(ClimbToTop);
        
        player.healthComponent.TriggerInvulnerability(2);
        
        SetColliderTrigger(false);
    }
}