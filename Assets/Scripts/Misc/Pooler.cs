using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class Pooler
{
    public GameObject ObjectToPool { get;}
    
    private readonly List<GameObject> _pooledObjects;
    private readonly GameObject _parent;
    private readonly IObjectResolver _container;
    
    public Pooler(GameObject objectToPool, int poolSize, IObjectResolver container, GameObject parent = null)
    {
        ObjectToPool = objectToPool;
        _pooledObjects = new List<GameObject>();
        _container = container;
        if(parent != null) _parent = parent;
        
        for (int i = 0; i < poolSize; i++)
        {
            var pooledObject = _container.Instantiate(ObjectToPool);
            pooledObject.SetActive(false);
            _pooledObjects.Add(pooledObject);
            if(_parent != null) pooledObject.transform.parent = _parent.transform;
        }
    }
    
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < _pooledObjects.Count; i++)
        {
            if (!_pooledObjects[i].activeInHierarchy)
            {
                return _pooledObjects[i];
            }
        }
        
        var pooledObject = _container.Instantiate(ObjectToPool);
        pooledObject.SetActive(false);
        _pooledObjects.Add(pooledObject);
        if(_parent != null) pooledObject.transform.parent = _parent.transform;
        
        return pooledObject;
    }

    public void ResetPooledObjects()
    {
        foreach (var pooledObject in _pooledObjects)
        {
            pooledObject.SetActive(false);
        }
    }
}
