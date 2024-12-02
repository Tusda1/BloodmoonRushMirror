using System;
using UnityEngine;
using Player;

public class LaserAttackState : DraculaState
{
    [SerializeField] private float trackingTime;
    [SerializeField] private float waitingTime;
    [SerializeField] private float firingTime;
    
    private Teleport _teleport;
    private Vector2 _teleportDestination;
    private Vector2 _firePosition;
    private Transform _player;
    private float _trackingTimer;
    private GameObject _trackerPrefab;
    private GameObject _trackerObject;
    private LineRenderer _lineRenderer;
    private float waitingTimer;
    private GameObject _laserPrefab;
    private float _firingTimer;
    private GameObject _laserObject;
    
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
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
        _player = ServiceLocator.Get<PlayerScript>().transform;
        _trackerPrefab = Resources.Load<GameObject>("Prefabs/LineRenderer");
        _laserPrefab = Resources.Load<GameObject>("Prefabs/Laser");
    }

    private void OnEnable()
    {
        // Find top of the room
        var hit = Physics2D.Raycast(transform.position, Vector2.up, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find roof"); return;}
        
        var roof = hit.point;
        roof.y -= 1.5f;
        
        // Find the right corner
        hit = Physics2D.Raycast(roof, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right corner"); return;}
        
        var rightCorner = hit.point;
        
        // Find the left corner
        hit = Physics2D.Raycast(roof, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        var leftCorner = hit.point;
        
        // Find middle point between left and right corner
        _teleportDestination = (rightCorner + leftCorner) / 2;
        
        stage = LaserAttackStage.Teleporting;
    }
    
    private void Update()
    {
        switch (stage)
        {
            case LaserAttackStage.Teleporting:
                if (!_teleport.IsTeleporting) _teleport.TeleportToLocation(_teleportDestination);
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
    
    private void OnTeleportEnd()
    {
        stage = LaserAttackStage.Tracking;
    }
    
    private void Tracking()
    {
        _trackingTimer += Time.deltaTime;
        if (_trackingTimer <= trackingTime)
        {
            _firePosition = _player.position;
            
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
        if (_laserObject == null)
        {
            _laserObject = Instantiate(_laserPrefab, transform.position, Quaternion.identity);
        }
        
        _firingTimer += Time.deltaTime;
        if (_firingTimer >= firingTime)
        {
            StateFinished = true;
        }
    }
}
