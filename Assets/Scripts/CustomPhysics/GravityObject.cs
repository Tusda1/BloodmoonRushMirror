using System;
using System.Collections.Generic;
using Attributes;
using Gaskellgames;
using UnityEngine;

public abstract class GravityObject : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField]
    protected Rigidbody2D rb2d;
    
    [Header("Gravity Object")] [SerializeField, ReadOnly]
    public bool isGrounded;

    [SerializeField, ReadOnly] public bool isFalling;

    [SerializeField] public Axis lockToAxis;

    public Vector2 targetVelocity;
    public Vector2 velocity;

    [Tooltip("If Gravidimodifyer is 0, set: GroundNormal = new Vector2 (0f, 1f); in start function in inherited class")]
    public float GravityModifier = 1f;

    protected Vector2 GroundNormal; //set this to Vector2.Right in child if you want to make projectiles

    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private readonly List<RaycastHit2D> _hitBufferList = new(16);
    private const float MinMoveDistance = 0.001f;
    private const float ShellRadius = 0.01f; //0.01
    private ContactFilter2D _contactFilter;
    private const float MinGroundNormalY = 0.7f; //0.65f
    #endregion

    protected void Awake() {
        rb2d ??= GetComponent<Rigidbody2D>();
    }

    protected virtual void Start() {
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        //contactFilter.SetLayerMask(layerMask);
        _contactFilter.useLayerMask = true;
        GroundNormal = new Vector2(0f, 1f);
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
            _hitBufferList.Clear();
    
            // Get all colliders attached to the Rigidbody2D
            var attachedColliders = new Collider2D[16]; // Create an array with 16 spaces to store colliders
            int colliderCount = rb2d.GetAttachedColliders(attachedColliders, true);
            Array.Resize(ref attachedColliders, colliderCount); // Resize the array to the number of found colliders
    
            foreach (var attachedCollider in attachedColliders) {
                if (attachedCollider.isTrigger) continue; // Skip trigger colliders
    
                // Create a ContactFilter2D for the collider's layer
                var contactFilter = new ContactFilter2D {
                    useTriggers = false,
                    layerMask = Physics2D.GetLayerCollisionMask(attachedCollider.gameObject.layer),
                    useLayerMask = true
                };
    
                // Perform the cast for the current collider
                var count = attachedCollider.Cast(move, contactFilter, _hitBuffer, distance + ShellRadius);
    
                for (var i = 0; i < count; i++) {
                    _hitBufferList.Add(_hitBuffer[i]);
                } 
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

        if (move.normalized.y * distance < 0 && !isGrounded) isFalling = true;
        else isFalling = false;
    
        transform.position = lockToAxis switch {
            Axis.None => rb2d.position + move.normalized * distance,
            Axis.XAxis => new Vector2(rb2d.position.x + move.normalized.x * distance, rb2d.position.y),
            Axis.YAxis => new Vector2(rb2d.position.x, rb2d.position.y + move.normalized.y * distance),
            _ => rb2d.position
        };
    }

    public void ResetMovement(Axis axis) {
        velocity = axis switch {
            Axis.None => velocity,
            Axis.XAxis | Axis.YAxis => Vector2.zero,
            Axis.XAxis => new Vector2(0, velocity.y),
            Axis.YAxis => new Vector2(velocity.x, 0),
            _ => velocity
        };
    }

    [Flags] public enum Axis { None = 0, XAxis = 1, YAxis = 2 }
}