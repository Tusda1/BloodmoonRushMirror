using UnityEngine;

public class BossDoorController : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private ConversationTrigger conversationTrigger;
    [SerializeField] private BossBase bossBase;

    private bool _state;
    
    private static readonly int DoorClosed = Animator.StringToHash("DoorClosed");
    #endregion

    public void TriggerDoor() {
        if (_state) return;
        
        doorAnimator.SetBool(DoorClosed, true);
        _state = true;

        if (conversationTrigger.hasBeenTriggered) {
            bossBase.StartBattle();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player") || !_state) return;
        
        doorAnimator.SetBool(DoorClosed, false);
        _state = false;
    }
}