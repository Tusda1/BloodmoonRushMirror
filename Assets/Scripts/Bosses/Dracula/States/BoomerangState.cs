using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoomerangState : DraculaState
{
    [SerializeField] private AnimationClip boomerangThrow;
    [SerializeField] private AnimationClip boomerangCatch;
    
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
            _rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
            _teleportPosition = _rightCorner;
        }
        else
        {
            _leftCorner.x += 1;
            _leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
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
    }
    
    private void Boomerang()
    {
        if (!_boomerangThrown)
        {
            _boomerang.transform.position = transform.position;
            _boomerang.direction = _teleportPosition == _rightCorner ? Vector2.left : Vector2.right;
            _boomerang.gameObject.SetActive(true);
            _boomerangThrown = true;
            Boss.AnimancerComponent.Play(boomerangThrow);
        }
        
        _boomerangStage = BoomerangStage.Wait;
    }
    
    private void Wait()
    {
        if (_boomerang.returned)
        {
            var state = Boss.AnimancerComponent.Play(boomerangCatch);
            state.Events(this).OnEnd = () =>
            {
                _boomerangThrown = false;
                EndState();
            };
        }
    }
}
