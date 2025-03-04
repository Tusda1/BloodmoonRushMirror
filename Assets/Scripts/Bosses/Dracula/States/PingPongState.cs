using System;
using System.Linq;
using UnityEngine;
using Player;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PingPongState : DraculaState
{
    [SerializeField] private AnimationClip pingPongAnimation;
    [SerializeField] private AnimationClip receiveWindupAnimation;
    [SerializeField] private AnimationClip receiveAnimation;
    [SerializeField] private float pingPongStartSpeed = 5f;
    [SerializeField] private int pingPongsBeforeFailure = 3;
    
    private bool _readyToPingPong;
    private GameObject _pingPongCircle;
    private GameObject _player;
    private int _pingPongHitCount;
    private Vector2 _directionToPingPong;
    private Vector2 _teleportPosition;
    
    private CornerFinder _cornerFinder;
    
    private enum PingPong
    {
        Teleport,
        Spawn,
        Play
    }
    
    private PingPong _pingPongState;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        //Initialize variables
        _pingPongCircle = Resources.Load<GameObject>("Prefabs/PingPongCircle");
        _player = ServiceLocator.Get<PlayerScript>().gameObject;
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        rightCorner.x -= 1f;
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        leftCorner.x += 1f;
        
        // 50% chance to go left or right
        if (Random.Range(0, 2) == 0)
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
        switch (_pingPongState)
        {
            case PingPong.Teleport:
                if (!teleport.IsTeleporting)
                {
                    var teleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
                    teleport.TeleportToLocation(_teleportPosition, teleportEnter, teleportExit);
                }
                break;
            case PingPong.Spawn:
                SpawnPingPongCircle();
                break;
            case PingPong.Play:
                PlayPingPong();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _pingPongState = PingPong.Spawn;
    }
    
    private void SpawnPingPongCircle()
    {
        if (!_readyToPingPong)
        {
            if (!Boss.AnimancerComponent.IsPlaying(pingPongAnimation))
            {
                Boss.facingDirection = _player.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
                var state = Boss.AnimancerComponent.Play(pingPongAnimation);
                state.Events(this).OnEnd = () =>
                {
                    _readyToPingPong = true;
                    state.Destroy();
                };
            }
            
            return;
        }
        
        // Spawn ping pong circle and send it to the player
        var position = transform.position + Vector3.up * 2.25f + (Vector3)Boss.facingDirection * 1.8f;
        GameObject pingPongCircle = Instantiate(_pingPongCircle, position, Quaternion.identity);
        
        // send the circle in the x direction of the player with no y movement
        pingPongCircle.GetComponent<PingPongScript>().setVelocity = Boss.facingDirection * pingPongStartSpeed;
        
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
            ReceivePingPong();
            DamageInstance damageInstance = new DamageInstance();
            damageInstance.knockback = _directionToPingPong;
            
            hit.collider.GetComponent<IDamageable>().TakeDamage(damageInstance);
        }
    }
    
    private void ReceivePingPong()
    {
        if (_pingPongHitCount <= pingPongsBeforeFailure)
        {
            var state = Boss.AnimancerComponent.Play(receiveWindupAnimation);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                Boss.AnimancerComponent.Play(receiveAnimation);
                
            };
        }
        else
        {
            var vulnerableStates = GetComponents<VulnerableState>();
            var vulnerableState = vulnerableStates.FirstOrDefault(state => state.StatePhase == StatePhase);
            Boss.StateMachine.ForceSetState(vulnerableState);
        }
        
        _pingPongHitCount++;
    }
    
    private void OnDisable()
    {
        Debug.Log("PingPongState Exit");
    }
}
