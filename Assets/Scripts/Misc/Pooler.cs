using System.Collections.Generic;
using UnityEngine;

public class Pooler
{
    private readonly List<GameObject> _pooledObjects;
    private readonly GameObject _objectToPool;
    private readonly GameObject _parent;
    
    public Pooler(GameObject objectToPool, int poolSize, GameObject parent = null)
    {
        _objectToPool = objectToPool;
        _pooledObjects = new List<GameObject>();
        if(parent != null) _parent = parent;
        
        for (int i = 0; i < poolSize; i++)
        {
            var pooledObject = Object.Instantiate(_objectToPool);
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
        
        var pooledObject = Object.Instantiate(_objectToPool);
        pooledObject.SetActive(false);
        _pooledObjects.Add(pooledObject);
        if(_parent != null) pooledObject.transform.parent = _parent.transform;
        
        return pooledObject;
    }
}
