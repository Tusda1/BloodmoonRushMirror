using UnityEngine;
using Player;
using UnityEngine.Serialization;

public class PingPongState : DraculaState
{
    private GameObject _pingPongCircle;
    private GameObject _player;
    private int _pingPongHitCount;
    private Vector2 _directionToPingPong;
    private Teleport _teleport;
    private Vector2 _teleportPosition;
    
    [SerializeField] private float pingPongStartSpeed = 5f;
    
    private enum PingPong
    {
        Teleport,
        Spawn,
        Play
    }
    
    private PingPong _pingPongState;
    
    private void OnEnable()
    {
        //Initialize variables
        _pingPongCircle = Resources.Load<GameObject>("Prefabs/PingPongCircle");
        _player = ServiceLocator.Get<PlayerScript>().gameObject;
        
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
        
        // Find bottom right and left corner by sending a ray straight down and then right
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find ground"); return;}
        
        var bottom = hit.point;
        bottom.y += 0.01f;
        
        hit = Physics2D.Raycast(bottom, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right corner"); return;}
        
        var rightCorner = hit.point;
        rightCorner.x -= 1f;
        rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // Find the left corner
        hit = Physics2D.Raycast(bottom, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        var leftCorner = hit.point;
        leftCorner.x += 1f;
        leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // 50% chance to go left or right
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            _teleportPosition = rightCorner;
        }
        else
        {
            _teleportPosition = leftCorner;
        }
        
        _pingPongState = PingPong.Teleport;
    }
    
    public void Update()
    {
        switch ((PingPong)_pingPongHitCount)
        {
            case PingPong.Teleport:
                if (_teleport.IsTeleporting) _teleport.TeleportToLocation(_teleportPosition);
                break;
            case PingPong.Spawn:
                SpawnPingPongCircle();
                break;
            case PingPong.Play:
                PlayPingPong();
                break;
        }
    }
    
    private void OnTeleportEnd()
    {
        _pingPongState = PingPong.Spawn;
    }
    
    private void SpawnPingPongCircle()
    {
        // Spawn ping pong circle and send it to the player
        var position = transform.position;
        GameObject pingPongCircle = Instantiate(_pingPongCircle, position, Quaternion.identity);
        
        // send the circle in the x direction of the player with no y movement
        _directionToPingPong = (_player.transform.position - position).normalized.x > 0 ? Vector2.right : Vector2.left;
        pingPongCircle.GetComponent<Rigidbody2D>().linearVelocity = _directionToPingPong * pingPongStartSpeed;
        
        _pingPongState = PingPong.Play;
    }
    
    private void PlayPingPong()
    {
        // Use Raycast to check if the pingpongcircle is close enough to be hit back
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _directionToPingPong, 1);
        if (hit.collider != null && hit.collider.CompareTag("PingPongCircle"))
        {
            if (hit.collider.attachedRigidbody.linearVelocity.normalized == _directionToPingPong)
            {
                return;
            }
            
            // Hit the circle back
            DamageInstance damageInstance = new DamageInstance();
            damageInstance.knockback = _directionToPingPong;
            
            hit.collider.GetComponent<IDamageable>().TakeDamage(damageInstance);
        }
    }
    
    private void OnDisable()
    {
        Debug.Log("PingPongState Exit");
    }
}
