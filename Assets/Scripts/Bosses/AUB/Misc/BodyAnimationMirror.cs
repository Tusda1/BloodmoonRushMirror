using System;
using Animancer;
using UnityEngine;

[Serializable]
public class AnimationPair
{
    public AnimationClip bodyAnimation;
    public AnimationClip headAnimation;
}

public class BodyAnimationMirror : MonoBehaviour 
{
    [SerializeField] private AnimationPair[] animationPairs;
    
    private AnimancerComponent _headAnimancer;
    private AnimancerComponent _animancer;
    private AnimationClip _currentHeadAnimation;

    private void Awake()
    {
        _headAnimancer = GetComponentInParent<BossBase>().AnimancerComponent;
        _animancer = GetComponent<AnimancerComponent>();
    }

    private void Update()
    {
        if (_currentHeadAnimation is not null && _currentHeadAnimation == _headAnimancer.States.Current?.Clip) return;
        
        foreach (var pair in animationPairs)
        {
            if (pair.headAnimation == _headAnimancer.States.Current?.Clip)
            {
                _animancer.Play(pair.bodyAnimation);
                _currentHeadAnimation = pair.headAnimation;
                return;
            }
        }
        
        Debug.LogError("No matching body animation found for the current head animation.");
    }
}