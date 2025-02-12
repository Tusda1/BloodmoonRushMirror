using Player;
using UnityEngine;
using VContainer;

public class CrusherScript : GravityObject {
    #region ---Fields---
    [HideInInspector] public float upSpeed;
    [HideInInspector] public float downSpeed;
    [HideInInspector] public Vector2 targetPosition;
    
    [SerializeField] private DamageInstance damageInstance = new();
    
    public Collider2D triggerCollider2D;
    public bool goingDown = false;
    public bool isStill = true;
    public Vector2 startPos;
    public Vector2 endPos;
    
    [Inject] private CameraEffects _cameraEffects;
    #endregion

    protected override void ComputeVelocity() {
        if (isStill) return;

        var currentPosition = new Vector2(transform.position.x, transform.position.y);

        float speed;

        if (goingDown) {
            targetPosition = endPos - currentPosition;
            speed = downSpeed;
        }
        else {
            targetPosition = startPos - currentPosition;
            speed = upSpeed;
        }

        targetVelocity = targetPosition.normalized * speed;

        if (goingDown) {
            if (IsInsideCircle(endPos, 0.2f, currentPosition)) {
                isStill = true;
                triggerCollider2D.isTrigger = false;
                _cameraEffects?.ShakeCamera(0.2f, 0.2f);
            }
        }

        else {
            if (IsInsideCircle(startPos, 0.2f, currentPosition))
                isStill = true;
        }
    }

    private bool IsInsideCircle(Vector2 center, float radius, Vector2 point) {
        if (Mathf.Pow((point.x - center.x), 2) + Mathf.Pow((point.y - center.y), 2) < Mathf.Pow(radius, 2)) {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.transform.TryGetComponent<IDamageable>(out var damageable)) return;
                
                switch (damageable) {
                    case PlayerScript playerScript:
                        playerScript.TakeDamage(damageInstance);
                        playerScript.playerMovement.SpikeReset();
                        break;
                    case EnemyBase enemyBase:
                        damageInstance.damage = 99;
                        enemyBase.TakeDamage(damageInstance);
                        damageInstance.damage = 1;
                        break;
        }
    }
}