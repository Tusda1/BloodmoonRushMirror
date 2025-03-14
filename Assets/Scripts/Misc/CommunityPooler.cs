using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[DefaultExecutionOrder(2)]
public class CommunityPooler : MonoBehaviour 
{
    public List<Pooler> poolers {get ;} = new();
    
    [SerializeField] private GameObject[] prefabs;

    [Inject]
    public void Construct(LifetimeScope currentScope) {
        foreach (var projectile in prefabs)
        {
            poolers.Add(new Pooler(projectile, 10, currentScope.Container, gameObject));
        }
    }
    
    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy() {
        ServiceLocator.Unregister<CommunityPooler>();
    }
}
