using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoomerangState : DraculaState
{
    [SerializeField] private GameObject boomerangPrefab;
    
    private Vector2 _teleportPosition;
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
        _boomerang = Instantiate(boomerangPrefab, transform.position, Quaternion.identity).GetComponent<Boomerang>();
        _boomerang.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        
        // 50% chance to go left or right
        if (Random.Range(0, 2) == 0)
        {
            _teleportPosition = rightCorner;
            rightCorner.x -= 1;
            rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        }
        else
        {
            _teleportPosition = leftCorner;
            leftCorner.x += 1;
            leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        }
        
        _boomerangStage = BoomerangStage.Teleport;
    }

    private void Update()
    {
        switch (_boomerangStage)
        {
            case BoomerangStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportPosition);
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
        _boomerang.direction = Vector2.right;
        _boomerang.gameObject.SetActive(true);
    }
    
    private void Wait()
    {
        if (_boomerang.returned)
        {
            EndState();
        }
    }
}
