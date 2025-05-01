using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class IceRain : DrucillaState
{
    [SerializeField] private AnimationClip iceRainAnimation;
    
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private int projectileSpawnAmount = 1;
    [SerializeField] private GameObject bigIciclePrefab;
    [SerializeField] private GameObject smallIciclePrefab;
    [SerializeField] private float spawnDistanceFromRoof = 2f;
    
    private float _timer;
    private float _cooldownTimer;
    private bool _hasTeleported;
    private bool _iceRainActive;
    private Vector2 cornerOne;
    private Vector2 cornerTwo;
    private Coroutine _waitOnEndCoroutine;
    
    private Pooler _bigPooler;
    private Pooler _smallPooler;
    private CornerFinder _cornerFinder;

    protected override void Awake()
    {
        base.Awake();
        
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        _bigPooler = TryGetCommunityPooler.TryGetPooler(bigIciclePrefab, ObjectResolver);
        _smallPooler = TryGetCommunityPooler.TryGetPooler(smallIciclePrefab, ObjectResolver);
    }
    
    private void OnEnable()
    {
        cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
    }

    private void OnDisable()
    {
        _hasTeleported = false;
        _iceRainActive = false;
    }

    private void Update()
    {
        if (!_hasTeleported)
        {
            if (!teleport.IsTeleporting) teleport.TeleportToLocation(new Vector2(Random.Range(cornerOne.x + 1, cornerTwo.x - 1), cornerOne.y - 5), DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
        }
        else
        {
            RainIce();
        }
    }

    private void RainIce()
    {
        if (!_iceRainActive)
        {
            if (!Boss.AnimancerComponent.IsPlaying(iceRainAnimation))
            {
                audioManager.PlayOneShotAttached("Druc_Icerain_Activate", Boss.gameObject);
                var state = Boss.AnimancerComponent.Play(iceRainAnimation);
                state.Events(this).OnEnd = () =>
                {
                    state.Destroy();
                    _iceRainActive = true; 
                    Boss.AnimancerComponent.Play(idleAnimation);
                };
            }
            
            return;
        }
        
        if (_timer > duration)
        {
            _timer = 0;
            if (_waitOnEndCoroutine == null)
            {
                _waitOnEndCoroutine = StartCoroutine(WaitOnEnd());
            }
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
        
        while (_cooldownTimer >= cooldown)
        {
            for (int i = 0; i < projectileSpawnAmount; i++)
            {
                var position = new Vector2(Random.Range(cornerOne.x, cornerTwo.x), cornerOne.y - spawnDistanceFromRoof);
                var projectile = Random.Range(0, 2) == 0 ? _bigPooler.GetPooledObject() : _smallPooler.GetPooledObject();
                projectile.transform.position = position;
                projectile.SetActive(true);
            }

            _cooldownTimer -= cooldown;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _hasTeleported = true;
    }

    private IEnumerator WaitOnEnd()
    {
        yield return new WaitForSeconds(1f);
        _waitOnEndCoroutine = null;
        EndState();
    }
}