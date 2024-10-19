using System;
using Fmod;
using UnityEngine;

public class CombatCheckpoint : MonoBehaviour
{
    public event Action CheckpointReached; 
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointReached?.Invoke();
            FmodManager.Instance.PlayOneShot("event:/Sfx/Misc/Checkpoint_reached");
            gameObject.SetActive(false);
        }
    }
}
