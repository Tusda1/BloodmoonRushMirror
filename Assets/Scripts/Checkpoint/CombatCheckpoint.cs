using System;
using Animancer;
using FMODScripts;
using UnityEngine;

public class CombatCheckpoint : MonoBehaviour
{
    [SerializeField] private AnimationClip inactiveIdle;
    [SerializeField] private AnimationClip activeIdle;
    [SerializeField] private AnimationClip activeStart;
    
    public event Action CheckpointReached;
    private bool _isCheckpointReached;
    private AnimancerComponent _animancerComponent;

    private void Start()
    {
        _animancerComponent = GetComponent<AnimancerComponent>();
        _animancerComponent.Play(inactiveIdle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isCheckpointReached)
        {
            CheckpointReached?.Invoke();
            FMODManager.RaiseSFXEvent("Misc/Checkpoint_reached");
            _isCheckpointReached = true;
            
            // Play animation
            var state = _animancerComponent.Play(activeStart);
            state.Events(this).OnEnd = () => _animancerComponent.Play(activeIdle);
        }
    }
}
