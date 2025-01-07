using UnityEngine;

namespace Triggers {
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class TriggerBox : MonoBehaviour {
        #region ---Fields---
        [SerializeField] private string colliderTag = "Player";

        protected bool HasBeenTriggered;
        #endregion

        private void OnTriggerEnter2D(Collider2D other) {
            if (HasBeenTriggered) return;
            if (colliderTag != null && !other.CompareTag(colliderTag)) return;

            TriggeredAction(other);
            HasBeenTriggered = true;
        }

        protected abstract void TriggeredAction(Collider2D other);
    }
}