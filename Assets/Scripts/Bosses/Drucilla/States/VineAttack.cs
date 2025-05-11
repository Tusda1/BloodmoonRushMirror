using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VineAttack : DrucillaState
{
    [SerializeField] private AnimationClip vineAttackAnimation;
    
    [SerializeField] private GameObject vinePrefab;
    [SerializeField] private float waitTimeAfterVineSpawn = 0.5f;
    [SerializeField] private int maxVines = 3;
    [SerializeField] private int minVines = 1;
    [SerializeField] private float distanceBetweenVines = 3f;
    
    private Vector2 _cornerOne;
    private Vector2 _cornerTwo;
    private int _vinesToSpawn;
    
    private Pooler _pooler;
    private CornerFinder _cornerFinder;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        _pooler = TryGetCommunityPooler.TryGetPooler(vinePrefab, objectResolver);
    }

    private void OnEnable()
    {
        _cornerOne = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        _cornerTwo = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        teleport.TeleportToLocation(new Vector2(Random.Range(_cornerOne.x + 1, _cornerTwo.x - 1), _cornerOne.y - 5), DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
        
        _vinesToSpawn = Random.Range(minVines, maxVines + 1);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    protected override void OnTeleportEnd()
    {
        var state = Boss.AnimancerComponent.Play(vineAttackAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            StartCoroutine(SpawnVines());
            Boss.AnimancerComponent.Play(idleAnimation);
        };
    }

    private IEnumerator SpawnVines()
    {
        var spawnPositions = new List<Vector2>();
        
        for (int i = 0; i < _vinesToSpawn; i++)
        {
            var vine = _pooler.GetPooledObject();
            var spawnPosition = Vector2.zero;
            var tries = 0;
            while (spawnPosition == Vector2.zero)
            {
                var randomX = Random.Range(_cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft).x, _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight).x);
                var potentialPosition = new Vector2(randomX, transform.position.y);
                if (spawnPositions.Count == 0)
                {
                    spawnPosition = potentialPosition;
                }
                else
                {
                    var canSpawn = true;
                    foreach (var position in spawnPositions)
                    {
                        if (Vector2.Distance(position, potentialPosition) < distanceBetweenVines)
                        {
                            canSpawn = false;
                            break;
                        }
                    }

                    if (canSpawn)
                    {
                        spawnPosition = potentialPosition;
                    }
                    else
                    {
                        tries++;
                        if (tries > 100)
                        {
                            Debug.LogError("VineAttack: Could not find a suitable spawn position");
                            break;
                        }
                    }
                }
            }
            spawnPositions.Add(spawnPosition);
            vine.transform.position = new Vector3(spawnPosition.x, vine.transform.position.y, 0);
            vine.SetActive(true);
        }
        
        yield return new WaitForSeconds(waitTimeAfterVineSpawn);
        EndState();
    }
}
