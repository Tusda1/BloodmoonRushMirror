using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoomerangState : DraculaState
{
    [SerializeField] private AnimationClip boomerangThrow;
    [SerializeField] private AnimationClip boomerangCatch;
    [SerializeField] private Vector2 boomerangSpawnOffset;
    
    private GameObject _boomerangPrefab;
    private bool _boomerangThrown;
    
    private Vector2 _teleportPosition;
    private Vector2 _leftCorner;
    private Vector2 _rightCorner;
    private Boomerang _boomerang;
    private CornerFinder _cornerFinder;
    
    private enum BoomerangStage
    {
        Teleport,
        Boomerang,
        Wait
    }
    
    private BoomerangStage _boomerangStage;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        _boomerangPrefab = Resources.Load<GameObject>("Prefabs/Boomerang");
        _boomerang = Instantiate(_boomerangPrefab, transform.position, Quaternion.identity).GetComponent<Boomerang>();
        _boomerang.gameObject.SetActive(false);
    }

    private void OnEnable()
    { 
        _rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        _leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        
        // 50% chance to go left or right
        if (Random.Range(0, 2) == 0)
        {
            _rightCorner.x -= 1;
            _teleportPosition = _rightCorner;
        }
        else
        {
            _leftCorner.x += 1;
            _teleportPosition = _leftCorner;
        }
        
        _boomerangStage = BoomerangStage.Teleport;
    }

    private void Update()
    {
        switch (_boomerangStage)
        {
            case BoomerangStage.Teleport:
                if (!teleport.IsTeleporting)
                {
                    var teleportEnter = Boss.bossGrounded ? teleportEnterGround : teleportEnterAir;
                    teleport.TeleportToLocation(_teleportPosition, teleportEnter, teleportExit);
                }
                break;
            case BoomerangStage.Boomerang:
                Boomerang();
                break;
            case BoomerangStage.Wait:
                Wait();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _boomerangStage = BoomerangStage.Boomerang;
        Boss.facingDirection = _teleportPosition == _rightCorner ? Vector2.left : Vector2.right;
    }
    
    private void Boomerang()
    {
        if (!_boomerangThrown)
        {
            var state = Boss.AnimancerComponent.Play(boomerangThrow);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                Boss.AnimancerComponent.Play(idleAnimation);
                _boomerang.transform.position = new Vector2(transform.position.x + boomerangSpawnOffset.x * Boss.facingDirection.x, transform.position.y + boomerangSpawnOffset.y);
                _boomerang.direction = _teleportPosition == _rightCorner ? Vector2.left : Vector2.right;
                _boomerang.gameObject.SetActive(true);
                _boomerangStage = BoomerangStage.Wait;
            };
            _boomerangThrown = true;
        }
    }
    
    private void Wait()
    {
        if (_boomerang.returned)
        {
            var state = Boss.AnimancerComponent.Play(boomerangCatch);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                Boss.AnimancerComponent.Play(idleAnimation);
                _boomerangThrown = false;
                _boomerang.returned = false;
                EndState();
            };
        }
    }
}
