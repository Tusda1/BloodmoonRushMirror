using UnityEngine;

public class BossDoorController : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private ConversationTrigger conversationTrigger;
    [SerializeField] private BossBase bossBase;
    
    private static readonly int DoorClosed = Animator.StringToHash("DoorClosed");
    #endregion

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        doorAnimator.SetBool(DoorClosed, true);

        if (conversationTrigger.hasBeenTriggered) {
            bossBase.StartBattle();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        doorAnimator.SetBool(DoorClosed, false);
    }
}
