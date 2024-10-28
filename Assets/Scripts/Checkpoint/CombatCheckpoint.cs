using System;
using FMODScripts;
using UnityEngine;

public class CombatCheckpoint : MonoBehaviour
{
    public event Action CheckpointReached; 
    
    private static FMODManager FMODManager => FMODManager.Instance;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointReached?.Invoke();
            FMODManager.PlaySfx("Misc/Checkpoint_reached");
            gameObject.SetActive(false);
        }
    }
}
