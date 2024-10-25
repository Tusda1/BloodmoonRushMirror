using Attributes;
using UnityEngine;

public class Ladder : MonoBehaviour {
    [Required] public Transform ladderStartPosition;
    [SerializeField] private BoxCollider2D platformCollider;
    [SerializeField] public BoxCollider2D triggerCollider;

    private bool ladderIsUsed;
    
    public bool LadderIsUsed {
        get => ladderIsUsed;
        set {
            platformCollider.enabled = !value;
            ladderIsUsed = value;
        }
    }
}