using System;
using System.Collections;
using UnityEngine;

public class BigLaserScript : MonoBehaviour
{
    [HideInInspector] public Vector2 direction;
    [SerializeField] private float speed = 1f;
    [SerializeField] private DamageInstance damageInstance;
    
    private BoxCollider2D _collider;
    
    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        // Send a raycast in direction to determine distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 100f, LayerMask.GetMask("Ground"));
        var targetScaleY = (hit.distance + 5) / _collider.size.y;
        
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x, targetScaleY, initialScale.z);
        float distance = Mathf.Abs(targetScaleY - initialScale.y);
        float duration = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            //_collider.size = transform.localScale;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
            Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;

            damageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * damageInstance.knockbackStrength;
            damageable.TakeDamage(damageInstance);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        var localScale = transform.localScale;
        transform.localScale = new Vector3(localScale.x, 0, localScale.z);
    }
}