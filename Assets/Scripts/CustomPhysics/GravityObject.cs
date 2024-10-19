using System;
using System.Collections.Generic;
using UnityEngine;

public class GravityObject : MonoBehaviour
{
    protected Vector2 targetVelocity;
    protected Vector2 velocity;
    protected Rigidbody2D rb2d;
    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected bool isGrounded;
    protected Vector2 groundNormal; //set this to Vector2.Right in child if you want to make projectiles

    protected float gravityModifier = 1f;
    protected float minGroundNormalY = 0.65f;
    protected int lockToAxis = 0; //0 for no, 1 for x, 2 for y :3

    protected void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    protected void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    protected void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {
        //use this in script with targetVelocity updated here :3
    }

    protected void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
        velocity.x = targetVelocity.x;
        
        isGrounded = false;
        
        Vector2 deltaPosition = velocity * Time.deltaTime;
        
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;
        
        Movement(move, false);
        
        move = Vector2.up * deltaPosition.y;
        
        Movement(move, true);
    }

    protected void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++) //checks if player is grounded
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if (currentNormal.y > minGroundNormalY)
                {
                    isGrounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }
                
                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }
                
                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
                
            }
            
        }
        
        

        switch (lockToAxis)
        {
            case 0:
                rb2d.position = rb2d.position + move.normalized * distance;
                break;
            case 1:
                rb2d.position = new Vector2((rb2d.position.x + move.normalized.x * distance), rb2d.position.y);
                break;
            case 2:
                rb2d.position = new Vector2(rb2d.position.x, (rb2d.position.y + move.normalized.y * distance));
                break;
        }
    }

    public void ResetMovement()
    {
        velocity = Vector2.zero;
    }

    public Vector2 GetVelocity()
    {
        return velocity;
    }
    
    /*
    Notes:
    
    
    
    */
}
