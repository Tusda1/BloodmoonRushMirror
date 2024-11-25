using System;
using UnityEngine;

public class BetterRuby : GravityObject
{
    [SerializeField] private float groundDistance;
    [SerializeField] private float hoverSpeed = 2f; // Speed of the hover motion
    [SerializeField] private float hoverHeight = 0.5f; // Height of the hover motion
    [SerializeField] private float despawnDistance = 0.1f; // Distance from the player at which the Ruby despawns
    [SerializeField] private float moveSpeed = 5f; // Speed at which the Ruby moves towards the player
    [SerializeField] private float pickupableTime = 0.2f; // Time after which the Ruby can be picked up
    public bool fall; // Whether the Ruby should fall to the ground or hover in place
    
    private float initialY;
    private float _spawnTime;
    private Transform _player;
    
    private enum RubyState
    {
        Hovering,
        Falling,
        Moving
    }
    
    private RubyState _state;

    private void OnEnable()
    {
        _spawnTime = Time.time;
        
        if (!fall)
        {
            GravityModifier = 0;
            _state = RubyState.Hovering;
        }
        else
        {
            _state = RubyState.Falling;
        }
    }

    private void Update()
    {
        switch (_state)
        {
            case RubyState.Falling:
                Fall();
                break;
            case RubyState.Hovering:
                Hover();
                break;
            case RubyState.Moving:
                Move();
                break;
        }
    }
    
    private void Fall()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundDistance, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            _state = RubyState.Hovering;
            GravityModifier = 0;
        }
    }
    
    private void Hover()
    {
        if (initialY == 0) initialY = transform.position.y;
        
        Vector3 position = transform.position;
        position.y = initialY + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = position;
    }
    
    private void Move()
    {
        if (Time.time - _spawnTime < pickupableTime) return;
        
        if (Vector3.Distance(transform.position, _player.position) > despawnDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime);
            moveSpeed *= 1 + Time.deltaTime;
        }
        else
        {
            // TODO: Add a sound effect for when the Ruby is picked up
            // TODO: Add to the gem counter in player
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.transform;
            _state = RubyState.Moving;
        }
    }
}
