using System;
using System.Linq;
using UnityEngine;
using Player;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PingPongState : DraculaState
{
    [SerializeField] private AnimationClip pingPongAnimation;
    [SerializeField] private AnimationClip receiveWindupAnimation;
    [SerializeField] private AnimationClip receiveAnimation;
    [SerializeField] private float pingPongStartSpeed = 5f;
    [SerializeField] private int pingPongsBeforeFailure = 3;
    [SerializeField] private float receiveDistance = 0.5f;
    
    private bool _readyToPingPong;
    private bool _isReceiving;
    private GameObject _pingPongCircle;
    private GameObject _player;
    private PingPongScript _pongPongInstance;
    private int _pingPongHitCount;
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
        _pongPongInstance = Instantiate(_pingPongCircle, transform.position, Quaternion.identity).GetComponent<PingPongScript>();
        
        
        
        _pongPongInstance.gameObject.SetActive(false);
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
                    Boss.AnimancerComponent.Play(idleAnimation);
                };
            }
            
            return;
        }
        
        // Spawn ping pong circle and send it to the player
        var position = transform.position + Vector3.up * 2.25f + (Vector3)Boss.facingDirection * 1.8f;
        _pongPongInstance.transform.position = position;
        // send the circle in the x direction of the player with no y movement
        _pongPongInstance.setVelocity = Boss.facingDirection * pingPongStartSpeed;
        _pongPongInstance.gameObject.SetActive(true);
        
        _pingPongState = PingPong.Play;
    }
    
    private void PlayPingPong()
    {
        if (!_pongPongInstance.gameObject.activeSelf)
        {
            EndState();
            return;
        }
        
        if (Mathf.Abs(_pongPongInstance.transform.position.x - transform.position.x) < receiveDistance)
        {
            // Hit the circle back
            ReceivePingPong();
        }
        else
        {
            _isReceiving = false;
        }
    }
    
    private void ReceivePingPong()
    {
        if (_isReceiving)
        {
            return;
        }
        
        print(_pingPongHitCount);
        if (_pingPongHitCount < pingPongsBeforeFailure)
        {
            _isReceiving = true;
            DamageInstance damageInstance = new DamageInstance();
            damageInstance.knockback = Boss.facingDirection;
            
            _pongPongInstance.GetComponent<IDamageable>().TakeDamage(damageInstance);
            var state = Boss.AnimancerComponent.Play(receiveAnimation);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                Boss.AnimancerComponent.Play(idleAnimation);
            };
        }
        else
        {
            var vulnerableStates = GetComponents<VulnerableState>();
            var vulnerableState = vulnerableStates.FirstOrDefault(state => state.StatePhase == StatePhase);
            Boss.StateMachine.ForceSetState(vulnerableState);
            return;
        }
        
        _pingPongHitCount++;
    }
    
    private void OnDisable()
    {
        Debug.Log("PingPongState Exit");
        _pingPongHitCount = 0;
        _readyToPingPong = false;
        _isReceiving = false;
        _pingPongState = PingPong.Teleport;
    }
}
