using System;
using UnityEngine;

public class PatrolEnemy : EnemyBase
{
    private void Update()
    {
        if (isDead) { return; }

        if (isStunned) { return; }

        Patrol();
    }
    
    private void Patrol()
    {
        // Move the enemy
        // Check if the enemy is at the edge of a platform
        // Change the enemy's facing direction
    }
}
