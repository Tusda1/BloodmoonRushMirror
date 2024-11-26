using System;
using Attributes;
using Player;
using UnityEngine;

public class SpikeScript : MonoBehaviour
{
    [SerializeField] private CompositeCollider2D spikeCollider;
    [SerializeField] private DamageInstance spikeDamageInstance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //var playerScript = other.gameObject.GetComponent<PlayerScript>();
            //playerScript.TakeDamage(spikeDamageInstance);
            var gravityObjectPlayerMovement = other.gameObject.GetComponent<GravityObjectPlayerMovement>();
            gravityObjectPlayerMovement.SpikeReset();
        }
    }
}
