using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class BombAttackState : DrucillaState
{
    [SerializeField] private AnimationClip bombDropAnimation;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float bombCooldown = 1f;
    [SerializeField] private float glideSpeed = 4f;
    [SerializeField] private float glideFallVelocity = -1f;
    [SerializeField] private float groundSlamDistanceThreshold = 3f;
    
    private float _bombTimer;
    private Vector2 _teleportPosition;
    private bool _droppingBombs;
    private float _groundHeight;
    
    private PlayerScript _player;
    
    [SerializeField] private GroundSlam groundSlam = new();
    private Pooler _bombPooler;
    
    private enum BombAttackStage
    {
        Teleport,
        DropBombs,
        GroundSlam
    }
    
    private BombAttackStage _stage;

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>();
        
        var tempCollider = GetComponent<Collider2D>();
        groundSlam.InitializeGroundSlam(tempCollider, Boss.AnimancerComponent);
        _bombPooler = TryGetCommunityPooler.TryGetPooler(bombPrefab, objectResolver);
    }

    private void OnEnable()
    {
        _bombTimer = 0;
        
        // send raycast to find the ground height
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            _groundHeight = hit.point.y;
        }
        
        _stage = BombAttackStage.Teleport;
    }

    private void OnDisable()
    {
        _droppingBombs = false;
        Boss.smoothDampScript.SetStartVelocity(Vector2.zero);
        groundSlam.ResetGroundSlam();
        _bombPooler.ResetPooledObjects();
    }

    private void Update()
    {
        switch (_stage)
        {
            case BombAttackStage.Teleport:
                if (!teleport.IsTeleporting)
                {
                    var hit = Physics2D.Raycast(_player.transform.position, Vector2.up, 100f, LayerMask.GetMask("Ground"));
                    _teleportPosition = hit.point + Vector2.down * 4f;
                    teleport.TeleportToLocation(_teleportPosition, DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
                }
                break;
            case BombAttackStage.DropBombs:
                DropBombs();
                break;
            case BombAttackStage.GroundSlam:
                groundSlam.ExecuteGroundSlam(ref Boss.targetVelocity, OnGroundSlamEnd);
                break;
        }
    }

    protected override void OnTeleportEnd()
    {
        _stage = BombAttackStage.DropBombs;
    }

    private void DropBombs()
    {
        if (!_droppingBombs)
        {
            Boss.AnimancerComponent.Play(bombDropAnimation);
            _droppingBombs = true;
        }
        
        if (_bombTimer < bombCooldown)
        {
            _bombTimer += Time.deltaTime;
        }
        else
        {
            _bombTimer = 0;
            SpawnBomb();
        }
        
        var direction = (_player.transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
        
        Boss.Move(new Vector2(direction.x * glideSpeed, glideFallVelocity));

        if (transform.position.y - _groundHeight < groundSlamDistanceThreshold)
        {
            _stage = BombAttackStage.GroundSlam;
        }
    }
    
    private void OnGroundSlamEnd()
    {
        EndState();
    }
    
    private void SpawnBomb()
    {
        audioManager.PlayOneShotAttached("Druc_Bomb_Drop", gameObject);
        var bomb = _bombPooler.GetPooledObject();
        bomb.transform.position = transform.position;
        bomb.SetActive(true);
    }
}
