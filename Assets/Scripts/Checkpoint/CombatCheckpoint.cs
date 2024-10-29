using System;
using FMODScripts;
using UnityEngine;

public class CombatCheckpoint : MonoBehaviour
{
    public event Action CheckpointReached; 
    private bool _isCheckpointReached;
    
    private static FMODManager FMODManager => FMODManager.Instance;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isCheckpointReached)
        {
            CheckpointReached?.Invoke();
            FMODManager.PlaySfx("Misc/Checkpoint_reached");
            _isCheckpointReached = true;
        }
    }
}
