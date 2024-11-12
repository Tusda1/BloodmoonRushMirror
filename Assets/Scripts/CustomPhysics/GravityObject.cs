using System.Collections.Generic;
using Attributes;
using Gaskellgames;
using UnityEngine;

public abstract class GravityObject : MonoBehaviour {
    [Header("Gravity Object")]
    [SerializeField, ReadOnly] protected bool isGrounded;
    [SerializeField] protected int lockToAxis; //0 for no, 1 for x, 2 for y :3

    protected Vector2 targetVelocity;
    protected Vector2 velocity;
    [GetOrAddComponent] protected Rigidbody2D rb2d;
    [SerializeField] protected float GravityModifier = 1f;
    protected Vector2 GroundNormal; //set this to Vector2.Right in child if you want to make projectiles

    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private readonly List<RaycastHit2D> _hitBufferList = new(16);
    private const float MinMoveDistance = 0.001f;
    private const float ShellRadius = 0.01f;
    private ContactFilter2D _contactFilter;
    private const float MinGroundNormalY = 0.65f;

    private void Awake() {
        rb2d ??= GetComponent<Rigidbody2D>();
    }

    protected virtual void Start() {
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        //contactFilter.SetLayerMask(layerMask);
        _contactFilter.useLayerMask = true;
    }

    protected void Update() {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    /// <summary>
    /// Override this method in scripts inheriting to run code in update
    /// </summary>
    protected virtual void ComputeVelocity() {
    }

    protected void FixedUpdate() {
        if (GravityModifier != 0) {
            velocity += Physics2D.gravity * (GravityModifier * Time.fixedDeltaTime);
        }
        else {
            velocity.y = targetVelocity.y;
        }

        velocity.x = targetVelocity.x;

        isGrounded = false;

        var deltaPosition = velocity * Time.deltaTime;
        var moveAlongGround = new Vector2(GroundNormal.y, -GroundNormal.x);
        var move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    private void Movement(Vector2 move, bool yMovement) {
        var distance = move.magnitude;

        if (distance > MinMoveDistance) {
            var count = rb2d.Cast(move, _contactFilter, _hitBuffer, distance + ShellRadius);
            _hitBufferList.Clear();

            for (var i = 0; i < count; i++) {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            foreach (var hit2D in _hitBufferList) {
                var currentNormal = hit2D.normal;
                if (currentNormal.y > MinGroundNormalY) {
                    isGrounded = true;
                    if (yMovement) {
                        GroundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                var projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0) {
                    velocity -= projection * currentNormal;
                }

                var modifiedDistance = hit2D.distance - ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        rb2d.position = lockToAxis switch {
            0 => rb2d.position + move.normalized * distance,
            1 => new Vector2(rb2d.position.x + move.normalized.x * distance, rb2d.position.y),
            2 => new Vector2(rb2d.position.x, rb2d.position.y + move.normalized.y * distance),
            _ => rb2d.position
        };
    }

    public void ResetMovement(int whichAxis) {
        velocity = whichAxis switch {
            0 => Vector2.zero,
            1 => new Vector2(0, velocity.y),
            2 => new Vector2(velocity.x, 0),
            _ => velocity
        };
    }
}