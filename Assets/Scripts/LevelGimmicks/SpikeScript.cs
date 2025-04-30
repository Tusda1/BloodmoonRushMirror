using System;
using Player;
using UnityEngine;

public class SpikeScript : MonoBehaviour {
    [SerializeField] private DamageInstance damageInstance = new();
    [SerializeField] private DamageInstance enemyDamage = new();

    private void Start() {
        enemyDamage.damage = 99;
    } 

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.transform.TryGetComponent<IDamageable>(out var damageable)) return;
        
        switch (damageable) {
            case PlayerScript playerScript:
                playerScript.TakeDamage(damageInstance);
                playerScript.playerMovement.SpikeReset();
                break;
            case EnemyBase enemyBase:
                if (!enemyBase.takesSpikeDamage) return;
                
                enemyBase.TakeDamage(enemyDamage);
                break;
        }
    }
}