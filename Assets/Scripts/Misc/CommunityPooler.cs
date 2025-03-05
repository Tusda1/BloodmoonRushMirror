using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class CommunityPooler : MonoBehaviour 
{
    public List<Pooler> poolers {get ;} = new();
    
    [SerializeField] private GameObject[] prefabs;
    
    private void Awake()
    {
        foreach (var projectile in prefabs)
        {
            poolers.Add(new Pooler(projectile, 10, gameObject));
        }
        
        ServiceLocator.Register(this);
    }

    private void OnDestroy() {
        ServiceLocator.Unregister<CommunityPooler>();
    }
}
