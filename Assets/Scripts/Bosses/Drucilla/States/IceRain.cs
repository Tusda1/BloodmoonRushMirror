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
    
    private Pooler _pooler;
    private CornerFinder _cornerFinder;
    
    private void Start()
    {
        _pooler = TryGetCommunityPooler.TryGetPooler(projectilePrefab);
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }
    
    private void OnEnable()
    {
        var cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        var cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        
        EndState();
    }

    private void Update()
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
        
        var cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        var cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        
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