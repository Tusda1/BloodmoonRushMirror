using UnityEngine;

public class AUBState : BossState
{
    [SerializeField] protected HoverHand hoverHand;
    
    public bool canBeMirrored;

    protected override void OnValidate()
    {
        base.OnValidate();
        hoverHand = GetComponent<HoverHand>();
    }
}