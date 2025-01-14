using System;
using Player;
using UnityEngine;

public class BetterRuby : GravityObject
{
    [SerializeField] private float groundDistance;
    [SerializeField] private float hoverSpeed = 2f; // Speed of the hover motion
    [SerializeField] private float hoverHeight = 0.5f; // Height of the hover motion
    [SerializeField] private float despawnDistance = 0.1f; // Distance from the player at which the Ruby despawns
    [SerializeField] private float moveSpeed = 5f; // Speed at which the Ruby moves towards the player
    [SerializeField] private float accelerationFactor = 2f; // Factor by which the move speed increases over time
    [SerializeField] private float pickupableTime = 0.2f; // Time after which the Ruby can be picked up
    public bool fall; // Whether the Ruby should fall to the ground or hover in place
    
    private float initialY;
    private float _spawnTime;
    private bool _startedMoving;
    private PlayerScript _player;
    private ParticleSystem _particleSystem;
    private GameObject _spriteGameObject;
    private float _initialGravityModifier;
    
    private enum RubyState
    {
        Hovering,
        Falling,
        Moving
    }
    
    private RubyState _state;

    protected new void Awake()
    {
        base.Awake();
        
        _particleSystem = GetComponentInParent<ParticleSystem>();
        _spriteGameObject = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        _spriteGameObject.SetActive(true);
        
        _spawnTime = Time.time;
        
        if (!fall)
        {
            SwitchToHovering();
        }
        else
        {
            _state = RubyState.Falling;
        }
    }

    private void OnDisable()
    {
        _spriteGameObject.SetActive(false);
        initialY = 0;
        _startedMoving = false;
        _particleSystem.Stop();
        GravityModifier = _initialGravityModifier;
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();
        
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
            SwitchToHovering();
        }
    }
    
    private void SwitchToHovering()
    {
        _state = RubyState.Hovering;
        _initialGravityModifier = GravityModifier;
        GravityModifier = 0;
    }
    
    private void Hover()
    {
        print("hover");
        if (initialY == 0) initialY = transform.position.y;
    
        Vector3 position = transform.position;
        position.y = initialY + Mathf.Sin(Time.time * hoverSpeed - Mathf.PI / 2) * hoverHeight;
        transform.position = position;
    }
    
    private void Move()
    {
        var playerPosition = _player.transform.position + (Vector3)Vector2.up;
        
        if (Time.time - _spawnTime < pickupableTime) return;
        
        if (!_startedMoving)
        {
            _startedMoving = true;
            _particleSystem.Play();
        }
        
        if (Vector3.Distance(transform.position, playerPosition) > despawnDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime);
            moveSpeed *= 1 + Time.deltaTime * accelerationFactor;
        }
        else
        {
            // TODO: Add a sound effect for when the Ruby is picked up
            _player.gems++;
            enabled = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.GetComponent<PlayerScript>();
            _state = RubyState.Moving;
        }
    }
}
