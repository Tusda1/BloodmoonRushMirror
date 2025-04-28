using System;
using Player;
using UnityEngine;

public class ShockwaveDamageScript : MonoBehaviour 
{
    [SerializeField] private DamageInstance damageInstance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerScript>();
            if (player != null)
            {
                // calculate knockback upwards
                damageInstance.knockback = Vector2.up * damageInstance.knockbackStrength;
                player.TakeDamage(damageInstance);
            }
        }
    }
}
