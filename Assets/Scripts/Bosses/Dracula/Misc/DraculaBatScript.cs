using System;
using Animancer;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class DraculaBatScript : GravityObject
{
    [SerializeField] private AnimationClip batAnimationEmerge;
    [SerializeField] private AnimationClip batAnimation;
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private float moveSpeed = 5f;

    [HideInInspector] public Vector2 targetVector = Vector2.zero;

    private EventInstance _batSoundInstance;
    private AnimancerComponent _animancerComponent;

    protected new void Awake()
    {
        base.Awake();
        
        GravityModifier = 0;
        _animancerComponent = GetComponent<AnimancerComponent>();
    }

    private void OnEnable()
    {
        _batSoundInstance = RuntimeManager.CreateInstance("event:/Sfx/Boss/Dracula/Drac_bat");
        _batSoundInstance.start();
        
        if (targetVector == Vector2.zero)
        {
            gameObject.SetActive(false);
            return;
        }
        
        var state = _animancerComponent.Play(batAnimationEmerge);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            _animancerComponent.Play(batAnimation);
        };
    }

    private void OnDisable()
    {
        _batSoundInstance.stop(STOP_MODE.ALLOWFADEOUT);
        targetVector = Vector2.zero;
    }
    
    protected override void ComputeVelocity()
    {
        targetVelocity = targetVector.normalized * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damageInstance);
        }
        else if (other.CompareTag("ground"))
        {
            gameObject.SetActive(false);
        }
    }
}
