using System;
using FMODScripts;
using Triggers;
using UnityEngine;
using Animancer;
using Player;

public class LevelCheckpoint : TriggerBox {
    #region ---Fields---
    [SerializeField] private string raiseSoundEventPath = "event:/Sfx/Misc/Checkpoint_reached";
    [SerializeField] private AnimationClip inactiveIdle;
    [SerializeField] private AnimationClip activeIdle;
    [SerializeField] private AnimationClip activeStart;

    public event Action CheckpointReached = delegate { };
    private AnimancerComponent _animancerComponent;
    #endregion

    private void Start() {
        _animancerComponent = GetComponent<AnimancerComponent>();
        _animancerComponent.Play(inactiveIdle);
    }

    protected override void TriggeredAction(Collider2D other) {
        FMODManager.RaiseEvent(new PlaySoundEvent(raiseSoundEventPath));
        CheckpointReached.Invoke();

        var state = _animancerComponent.Play(activeStart);
        state.Events(this).OnEnd = () => _animancerComponent.Play(activeIdle);
        
        other.GetComponent<PlayerScript>().CheckpointHealing();
    }
}