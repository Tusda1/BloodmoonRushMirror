using UnityEngine;

namespace Triggers {
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class TriggerBox : MonoBehaviour {
        [SerializeField] private string colliderTag = "Player";

        private bool _hasBeenTriggered;

        private void OnTriggerEnter2D(Collider2D other) {
            if (colliderTag != default && !other.CompareTag(colliderTag) && !_hasBeenTriggered) return;

            TriggeredAction();
        }

        public abstract void TriggeredAction();
    }
}