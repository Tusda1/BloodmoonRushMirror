using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private Animator doorAnimator;
    
    private static readonly int DoorClosed = Animator.StringToHash("DoorClosed");
    #endregion

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        doorAnimator.SetBool(DoorClosed, true);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        doorAnimator.SetBool(DoorClosed, false);
    }
}
