using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class IceRain : DrucillaState
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private int projectileSpawnAmount = 1;
    [SerializeField] private GameObject projectilePrefab;
    
    private float _timer;
    private float _cooldownTimer;
    private bool _hasTeleported;
    private Vector2 cornerOne;
    private Vector2 cornerTwo;
    
    private Pooler _pooler;
    private CornerFinder _cornerFinder;

    protected override void Awake()
    {
        base.Awake();
        
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        _pooler = TryGetCommunityPooler.TryGetPooler(projectilePrefab, ObjectResolver);
    }
    
    private void OnEnable()
    {
        cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
    }

    private void OnDisable()
    {
        _hasTeleported = false;
    }

    private void Update()
    {
        if (!_hasTeleported)
        {
            if (!teleport.IsTeleporting) teleport.TeleportToLocation(new Vector2(Random.Range(cornerOne.x + 1, cornerTwo.x - 1), cornerOne.y - 2), DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
        }
        else
        {
            if (_timer > duration)
            {
                _timer = 0;
                EndState();
                return;
            }

            _timer += Time.deltaTime;

            if (_cooldownTimer < cooldown)
            {
                _cooldownTimer += Time.deltaTime;
                return;
            }

            cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
            cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);

            for (int i = 0; i < projectileSpawnAmount; i++)
            {
                var position = new Vector2(Random.Range(cornerOne.x, cornerTwo.x), cornerOne.y);
                var projectile = _pooler.GetPooledObject();
                projectile.transform.position = position;
                projectile.SetActive(true);
            }

            _cooldownTimer = 0;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _hasTeleported = true;
    }
}