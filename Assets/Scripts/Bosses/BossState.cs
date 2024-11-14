using UnityEngine;

public abstract class BossState
{
    protected Animator animator;
    
    public void Initialize(Animator bossAnimator)
    {
        animator = bossAnimator;
    }
    
    public abstract void Enter();
    
    public abstract void Update();
    
    public abstract void Exit();
}
