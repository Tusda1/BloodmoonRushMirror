using UnityEngine;

public class Dracula : BossBase
{
    public Pooler Pooler { get; private set; }
    
    private GameObject _batPrefab;
    
    [SerializeField] private AnimationClip teleportEnter;
    public AnimationClip TeleportEnter => teleportEnter;
    [SerializeField] private AnimationClip teleportExit;
    public AnimationClip TeleportExit => teleportExit;

    protected override void Start()
    {
        base.Start();
        
        _batPrefab = Resources.Load<GameObject>("Prefabs/DraculaBat");
        Pooler = new Pooler(_batPrefab, 15);
        
        GravityModifier = 0;
        DisableGravity();
    }
}
