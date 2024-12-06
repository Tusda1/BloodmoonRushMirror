using UnityEngine;

public class Dracula : BossBase
{
    public Pooler pooler { get; private set; }
    
    private GameObject _batPrefab;

    protected override void Start()
    {
        base.Start();
        
        _batPrefab = Resources.Load<GameObject>("Prefabs/DraculaBat");
        pooler = new Pooler(_batPrefab, 15, gameObject);
        
        GravityModifier = 0;
        DisableGravity();
    }
}
