using Player;
using UnityEngine;

public class LadderMiddle : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<PlayerScript>().playerMovement.ladderMiddle = this;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<PlayerScript>().playerMovement.ladderMiddle = null;
    }
}