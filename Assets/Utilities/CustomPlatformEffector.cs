using Attributes;
using UnityEngine;

public class CustomPlatformEffector : MonoBehaviour {
    [GetOrAddComponent, SerializeField, Required] private Collider2D platformCollider;

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.collider.CompareTag("Player")) return;
        
        var rb = collision.collider.attachedRigidbody;
        var ignoreCondition = !(rb.linearVelocity.y <= 0);
        Physics2D.IgnoreCollision(collision.collider, platformCollider, ignoreCondition);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.collider.CompareTag("Player")) {
            Physics2D.IgnoreCollision(collision.collider, platformCollider, false);
        }
    }
}