using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoomerangState : DraculaState
{
    [SerializeField] private GameObject boomerangPrefab;
    
    private Vector2 _teleportPosition;
    private Boomerang _boomerang;
    
    private Teleport _teleport;
    private TeleportConditionalActivation _teleportConditionalActivation = new();
    
    private enum BoomerangStage
    {
        Teleport,
        Boomerang,
        Wait
    }
    
    private BoomerangStage _boomerangStage;

    private void Start()
    {
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
        _boomerang = Instantiate(boomerangPrefab, transform.position, Quaternion.identity).GetComponent<Boomerang>();
        _boomerang.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Find bottom right and left corner by sending a ray straight down and then right
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find ground");
            return;
        }

        var bottom = hit.point;
        bottom.y += 0.01f;

        hit = Physics2D.Raycast(bottom, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find right corner");
            return;
        }

        var rightCorner = hit.point;
        rightCorner.x -= 1f;
        rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);

        // Find the left corner
        hit = Physics2D.Raycast(bottom, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find left corner");
            return;
        }

        var leftCorner = hit.point;
        leftCorner.x += 1f;
        leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // 50% chance to go left or right
        if (Random.Range(0, 2) == 0)
        {
            _teleportPosition = rightCorner;
        }
        else
        {
            _teleportPosition = leftCorner;
        }
        
        _boomerangStage = BoomerangStage.Teleport;
    }

    private void Update()
    {
        switch (_boomerangStage)
        {
            case BoomerangStage.Teleport:
                if (!_teleport.IsTeleporting) _teleportConditionalActivation.ActivateTeleport(transform.position, _teleportPosition, OnTeleportEnd, _teleport.TeleportToLocation);
                break;
            case BoomerangStage.Boomerang:
                Boomerang();
                break;
            case BoomerangStage.Wait:
                Wait();
                break;
        }
    }
    
    private void OnTeleportEnd()
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
