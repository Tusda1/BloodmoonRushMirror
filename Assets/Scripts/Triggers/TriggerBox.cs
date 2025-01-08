using UnityEngine;

namespace Triggers {
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class TriggerBox : MonoBehaviour {
        #region ---Fields---
        [SerializeField] private string colliderTag = "Player";

        [SerializeField] protected bool hasBeenTriggered;
        #endregion

        private void OnTriggerEnter2D(Collider2D other) {
            if (hasBeenTriggered) return;
            if (colliderTag != null && !other.CompareTag(colliderTag)) return;

            TriggeredAction(other);
            hasBeenTriggered = true;
        }

        protected abstract void TriggeredAction(Collider2D other);
    }
}