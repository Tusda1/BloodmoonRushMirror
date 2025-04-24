using System;
using UnityEngine;
using Player;

public class LaserAttackState : DraculaState
{
    [SerializeField] private AnimationClip laserAttackAnimation;
    [SerializeField] private float trackingTime;
    [SerializeField] private float firingTime;
    [SerializeField] private Vector2 laserOriginOffset;
    
    private Vector2 _teleportDestination;
    private Vector2 _firePosition;
    private GravityObjectPlayerMovement _player;
    private Collider2D _playerCollider;
    private GameObject _trackerPrefab;
    private GameObject _trackerObject;
    private LineRenderer _lineRenderer;
    private GameObject _laserPrefab;
    private float _timer;
    private GameObject _laserObject;
    
    private bool _hasStartedFiring;
    
    private CornerFinder _cornerFinder;
    
    private enum LaserAttackStage
    {
        Teleporting,
        Tracking,
        Firing
    }
    
    private LaserAttackStage stage;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>().GetComponent<GravityObjectPlayerMovement>();
        _playerCollider = _player.GetComponent<Collider2D>();
        _trackerPrefab = Resources.Load<GameObject>("Prefabs/LineRenderer");
        _laserPrefab = Resources.Load<GameObject>("Prefabs/BigLaser");
        
        if (_laserObject == null)
        {
            _laserObject = Instantiate(_laserPrefab, transform.position, Quaternion.identity);
            _laserObject.SetActive(false);
            
            //Set as child of boss
            _laserObject.transform.parent = transform;
            _laserObject.transform.localScale = new Vector3(1, 0, 1);
        }
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        
        // Find middle point between left and right corner
        _teleportDestination = (rightCorner + leftCorner) / 2;
        _teleportDestination.y -= 1.5f;
        
        stage = LaserAttackStage.Teleporting;
    }

    private void OnDisable()
    {
        _timer = 0;
        _hasStartedFiring = false;
    }

    private void Update()
    {
        switch (stage)
        {
            case LaserAttackStage.Teleporting:
                if (!teleport.IsTeleporting)
                {
                    var teleportEnter = Boss.bossGrounded ? teleportEnterGround : teleportEnterAir;
                    teleport.TeleportToLocation(_teleportDestination, teleportEnter, teleportExit);
                }
                break;
            case LaserAttackStage.Tracking:
                Tracking();
                break;
            case LaserAttackStage.Firing:
                Fire();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        Boss.AnimancerComponent.Play(idleAnimation);
        stage = LaserAttackStage.Tracking;
        // TODO : Play one shot. ID = "Drac_Lazer_Fire"
    }
    
    private void Tracking()
    {
        var animPlayed = false;
        _timer += Time.deltaTime;
        if (_timer >= trackingTime)
        {
            _timer = -10f;
            var state = Boss.AnimancerComponent.Play(laserAttackAnimation);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                stage = LaserAttackStage.Firing;
                Destroy(_trackerObject);
                _timer = 0;
                Boss.AnimancerComponent.Play(idleAnimation);
                // TODO : Play one shot. ID = "Drac_Lazer_Hold"
            };
        }
       
        _firePosition = _playerCollider.bounds.center;
            
        if (_trackerObject == null)
        {
            _trackerObject = Instantiate(_trackerPrefab, _firePosition, Quaternion.identity);
            _lineRenderer = _trackerObject.GetComponent<LineRenderer>();
        }
            
        _lineRenderer.SetPosition(0, GetComponent<Collider2D>().bounds.center);
        _lineRenderer.SetPosition(1, _firePosition);
    }
    
    private void Fire()
    {
        if (!_hasStartedFiring)
        {
            _firePosition = _playerCollider.bounds.center;
            if (_player.velocity != Vector2.zero)
            {
                _firePosition += _player.isFacingRight ? Vector2.right : Vector2.left;
            }
            var position = transform.position;
            _laserObject.GetComponent<BigLaserScript>().direction = (_firePosition - (Vector2)position).normalized;
            _laserObject.transform.position = (Vector2)position + laserOriginOffset;
            _laserObject.SetActive(true);
            

            // Calculate the angle to point the top of the laser object towards the fire position
            var direction = (_firePosition - (Vector2)_laserObject.transform.position).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees to point the top towards the fire position
            _laserObject.transform.rotation = Quaternion.Euler(0, 0, angle);
    
            _hasStartedFiring = true;
        }
        
        _timer += Time.deltaTime;
        if (_timer >= firingTime)
        {
            // TODO : End one shot. ID = "Drac_Lazer_Hold"
            _laserObject.SetActive(false);
            EndState();
            _timer = 0;
        }
    }
}
