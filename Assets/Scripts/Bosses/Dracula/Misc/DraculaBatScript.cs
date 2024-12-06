using System;
using UnityEngine;
using UnityEngine.Serialization;

public class DraculaBatScript : GravityObject
{
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private float moveSpeed = 5f;

    [HideInInspector] public Vector2 targetVector = Vector2.zero;

    protected override void Start()
    {
        base.Start();
        
        GravityModifier = 0;
    }

    private void OnEnable()
    {
        if (targetVector == Vector2.zero)
        {
            Debug.LogError("TargetVector is not set");
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        targetVector = Vector2.zero;
    }
    
    protected override void ComputeVelocity()
    {
        targetVelocity = targetVector.normalized * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damageInstance);
        }
        else if (other.CompareTag("ground"))
        {
            gameObject.SetActive(false);
        }
    }
}
