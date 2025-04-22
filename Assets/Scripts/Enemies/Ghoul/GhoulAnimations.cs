using UnityEngine;

public class GhoulAnimations : EnemyAnimationBase
{
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip run;
    [SerializeField] private AnimationClip jump;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(idle);
    }

    public void PlayRunAnimation()
    {
        animancerComponent.Play(run);
    }

    public void PlayJumpAnimation()
    {
        animancerComponent.Play(jump);
    }
}
