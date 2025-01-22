using System;
using UnityEngine;
using Player;

public class LaserAttackState : DraculaState
{
    [SerializeField] private float trackingTime;
    [SerializeField] private float waitingTime;
    [SerializeField] private float firingTime;
    
    private Vector2 _teleportDestination;
    private Vector2 _firePosition;
    private Transform _player;
    private Collider2D _playerCollider;
    private float _trackingTimer;
    private GameObject _trackerPrefab;
    private GameObject _trackerObject;
    private LineRenderer _lineRenderer;
    private float waitingTimer;
    private GameObject _laserPrefab;
    private float _firingTimer;
    private GameObject _laserObject;
    
    private bool _hasStartedFiring;
    
    private CornerFinder _cornerFinder;
    
    private enum LaserAttackStage
    {
        Teleporting,
        Tracking,
        Waiting,
        Firing
    }
    
    private LaserAttackStage stage;

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>().transform;
        _playerCollider = _player.GetComponent<Collider2D>();
        _trackerPrefab = Resources.Load<GameObject>("Prefabs/LineRenderer");
        _laserPrefab = Resources.Load<GameObject>("Prefabs/BigLaser");
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        
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

    private void Update()
    {
        switch (stage)
        {
            case LaserAttackStage.Teleporting:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportDestination);
                break;
            case LaserAttackStage.Tracking:
                Tracking();
                break;
            case LaserAttackStage.Waiting:
                WaitBeforeFire();
                break;
            case LaserAttackStage.Firing:
                Fire();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        stage = LaserAttackStage.Tracking;
    }
    
    private void Tracking()
    {
        _trackingTimer += Time.deltaTime;
        if (_trackingTimer <= trackingTime)
        {
            _firePosition = _playerCollider.bounds.center;
            
            if (_trackerObject == null)
            {
                _trackerObject = Instantiate(_trackerPrefab, _firePosition, Quaternion.identity);
                _lineRenderer = _trackerObject.GetComponent<LineRenderer>();
            }
            
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, _firePosition);
        }
        else
        {
            Destroy(_trackerObject);
            stage = LaserAttackStage.Waiting;
        }
    }

    private void WaitBeforeFire()
    {
        waitingTimer += Time.deltaTime;
        if (waitingTimer >= waitingTime)
        {
            stage = LaserAttackStage.Firing;
        }
    }
    
    private void Fire()
    {
        if (!_hasStartedFiring)
        {
            var position = transform.position;
            _laserObject.GetComponent<BigLaserScript>().direction = (_firePosition - (Vector2)position).normalized;
            _laserObject.transform.position = position;
            _laserObject.SetActive(true);
            

            // Calculate the angle to point the top of the laser object towards the fire position
            var direction = (_firePosition - (Vector2)position).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees to point the top towards the fire position
            _laserObject.transform.rotation = Quaternion.Euler(0, 0, angle);
    
            _hasStartedFiring = true;
        }
        
        _firingTimer += Time.deltaTime;
        if (_firingTimer >= firingTime)
        {
            _laserObject.SetActive(false);
            EndState();
        }
    }
}
