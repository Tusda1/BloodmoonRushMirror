using UnityEngine;

public class LadderMiddle : MonoBehaviour, ILadder {
    #region ---Fields---
    public CompositeCollider2D ladderCollider;
    #endregion

    public void SetColliderTrigger(bool value) {
        ladderCollider.isTrigger = value;
    }
}