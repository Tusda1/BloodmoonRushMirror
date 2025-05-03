using UnityEngine;

public class BossDoorTrigger : MonoBehaviour {
    [SerializeField] private BossDoorController bossDoorController;
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        
        bossDoorController.TriggerDoor();
    }
}