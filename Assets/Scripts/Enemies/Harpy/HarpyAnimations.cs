using UnityEngine;

public class HarpyAnimations : EnemyAnimationBase
{
    [SerializeField] private AnimationClip fly;
    [SerializeField] private AnimationClip swoop;
    
    public override void PlayDefaultAnimation()
    {
        if (animancerComponent.IsPlaying(fly)) return;
        animancerComponent.Play(fly);
    }
    
    public void PlaySwoopAnimation()
    {
        if (animancerComponent.IsPlaying(swoop)) return;
        animancerComponent.Play(swoop);
    }
}
