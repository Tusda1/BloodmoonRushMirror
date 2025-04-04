using UnityEngine;

public class AUBState : BossState
{
    [SerializeField] protected HoverHand hoverHand;
    
    public bool canBeMirrored;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        hoverHand = GetComponent<HoverHand>();
    }
#endif
}