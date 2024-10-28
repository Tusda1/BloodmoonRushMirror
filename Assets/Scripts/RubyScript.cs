using System;
using System.Collections;
using UnityEngine;

public class RubyScript : GravityObject
{
    [SerializeField] private float groundDistance;
    [SerializeField] private float hoverSpeed = 2f; // Speed of the hover motion
    [SerializeField] private float hoverHeight = 0.5f; // Height of the hover motion
    [SerializeField] private float despawnDistance = 0.1f; // Distance from the player at which the Ruby despawns
    [SerializeField] private float moveSpeed = 5f; // Speed at which the Ruby moves towards the player
    public bool fall; // Whether the Ruby should fall to the ground or hover in place
    
    private float initialY;
    private bool _canBePickedUp;
    private bool _isPickedUp;
    private Coroutine _currentCoroutine;
    private Transform _player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private new void Start()
    {
        base.Start();

        StartCoroutine(SpawnKillSafeguard());
        
        if (!fall)
        {
            _currentCoroutine = StartCoroutine(Hover());
        }
        else
        {
            _currentCoroutine = StartCoroutine(Fall());
        }
    }
    
    private IEnumerator SpawnKillSafeguard()
    {
        yield return new WaitForSeconds(0.2f);
        _canBePickedUp = true;
    }

    private IEnumerator Fall()
    {
        while (GravityModifier != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundDistance, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(Hover());
            }
            yield return null;
        }
    }

    private IEnumerator Hover()
    {
        initialY = transform.position.y;
        GravityModifier = 0;
        
        while (true)
        {
            Vector3 position = transform.position;
            position.y = initialY + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = position;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.transform;
            
            if(_isPickedUp){ return; }
            
            if (_canBePickedUp)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(FlyToPlayer());
            }
            else
            {
                StartCoroutine(WaitToBePickedUp());
            }
            
            _isPickedUp = true;
        }
    }
    
    private IEnumerator WaitToBePickedUp()
    {
        yield return new WaitUntil((() => _canBePickedUp));
        
        StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(FlyToPlayer());
    }
    
    private IEnumerator FlyToPlayer()
    {
        while (Vector3.Distance(transform.position, _player.position) > despawnDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        // TODO: Add a sound effect for when the Ruby is picked up
        // TODO: Add to the gem counter in player
        Destroy(gameObject);
    }
}