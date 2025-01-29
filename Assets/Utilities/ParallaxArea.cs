using Attributes;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class ParallaxArea : MonoBehaviour {
    #region ---Fields---
    [SerializeField, HideInInspector, GetOrAddComponent] private Parallax parallax;
    #endregion

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        
        parallax.Enable();
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        
        parallax.Disable();
    }
}
