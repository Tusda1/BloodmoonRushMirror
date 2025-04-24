using UnityEngine;

public class GhostAnimations : EnemyAnimationBase
{
    [SerializeField] private AnimationClip floatingAnimation;
    [SerializeField] private AnimationClip gettingSuckedAnimation;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(floatingAnimation);
    }

    public void PlayGettingSuckedAnimation()
    {
        if (animancerComponent.IsPlaying(gettingSuckedAnimation)) return;
        animancerComponent.Play(gettingSuckedAnimation);
    }
}
