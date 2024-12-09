using UnityEngine;

public class LadderMiddle : MonoBehaviour, ILadder {
    #region ---Fields---
    [SerializeField] private CompositeCollider2D ladderCollider;
    public LadderTop ladderTop;
    #endregion

    public void SetColliderTrigger(bool value) {
        ladderCollider.isTrigger = value;
    }
}