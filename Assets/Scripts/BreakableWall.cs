using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BreakableWall : MonoBehaviour, IDamageable {
    #region ---Fields---
    [SerializeField] private Explodable explodable;
    [SerializeField] private int health = 3;

    private GameObject[] _fragments;
    #endregion

    private void Start() {
        _fragments = explodable.fragments.ToArray();
    }

    public void TakeDamage(DamageInstance damageInstance) {
        health -= damageInstance.damage;
        Debug.Log("took damage: " + damageInstance.damage);

        if (health <= 0) {
            Debug.Log("Exploded!");
            _ = DeleteFragments(_fragments, 5, 2.5f);
            explodable.Explode();
        }
    }

    private static async Awaitable DeleteFragments(GameObject[] fragments, float duration, float delay = 0) {
        await Awaitable.WaitForSecondsAsync(delay);
        
        var waitTime = duration / fragments.Length;

        foreach (var fragment in fragments) {
            Destroy(fragment);
            Debug.Log("Destroyed fragment: " + fragment);
            await Awaitable.WaitForSecondsAsync(waitTime);
        }
    }
}