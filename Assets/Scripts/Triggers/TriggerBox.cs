using UnityEngine;

namespace Triggers {
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class TriggerBox : MonoBehaviour {
        #region ---Fields---
        [SerializeField] private string colliderTag = "Player";

        private bool _hasBeenTriggered;
        #endregion

        private void OnTriggerEnter2D(Collider2D other) {
            if (_hasBeenTriggered) return;
            if (colliderTag != default && !other.CompareTag(colliderTag)) return;

            TriggeredAction();
            _hasBeenTriggered = true;
        }

        public abstract void TriggeredAction();
    }
}