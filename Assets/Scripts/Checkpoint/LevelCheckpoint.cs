using System;
using FMODScripts;
using Triggers;
using UnityEngine;

public class LevelCheckpoint : TriggerBox {
    #region ---Fields---
    [SerializeField] private string raiseSoundEventPath = "event:/Sfx/Misc/Checkpoint_reached";
    
    public event Action CheckpointReached = delegate { };
    #endregion

    public override void TriggeredAction(Collider2D other) {
        FMODManager.RaiseEvent(new PlaySoundEvent(raiseSoundEventPath));
        CheckpointReached.Invoke();
    }
}