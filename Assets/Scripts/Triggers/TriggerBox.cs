using UnityEngine;

namespace Triggers {
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class TriggerBox : MonoBehaviour {
        #region ---Fields---
        [Header(nameof(TriggerBox))]
        [SerializeField] private string colliderTag = "Player";

        [SerializeField] public bool hasBeenTriggered;
        [SerializeField] protected bool triggerOnlyOnce = true;
        #endregion

        private void OnTriggerEnter2D(Collider2D other) {
            if (hasBeenTriggered && triggerOnlyOnce) return;
            if (colliderTag != null && !other.CompareTag(colliderTag)) return;

            hasBeenTriggered = true;
            TriggeredAction(other);
        }

        protected abstract void TriggeredAction(Collider2D other);
    }
}