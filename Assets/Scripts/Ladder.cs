using Attributes;
using UnityEngine;

public class Ladder : MonoBehaviour {
    [Required] public Transform ladderStartPosition;
    [SerializeField] private BoxCollider2D platformCollider;
    [SerializeField] public BoxCollider2D triggerCollider;

    public bool LadderIsUsed {
        get => !platformCollider.enabled;
        set => platformCollider.enabled = !value;
    }
}