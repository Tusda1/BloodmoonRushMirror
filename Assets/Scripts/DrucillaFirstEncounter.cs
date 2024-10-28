using DG.Tweening;
using FMODScripts;
using Unity.Behavior;
using UnityEngine;

public class DrucillaFirstEncounter : MonoBehaviour, IDamageable {
    [SerializeField] private StartNewConversationEventChannel startNewConversationEvent;
    [SerializeField] private BehaviorGraph bossDialogue;
    [SerializeField] private ActionEventChannel actionEvent;
    
    [SerializeField] private Animator floorAnimator;
    [SerializeField] private AnimationClip floorAnimation;

    private static FMODManager FMODManager => FMODManager.Instance;
    
    private Animator _animator;

    private void Start() {
        _animator = GetComponent<Animator>();
        
        transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 0.5f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void TakeDamage(DamageInstance damageInstance) {
        var dialogueVariables = bossDialogue.BlackboardReference.Blackboard.Variables;
        dialogueVariables.Find(x => x.Type == typeof(DrucillaFirstEncounter)).ObjectValue = this;
        dialogueVariables.Find(x => x.Type == typeof(Animator)).ObjectValue = _animator;
        startNewConversationEvent.SendEventMessage(bossDialogue);
        DOTween.Kill(transform);
        
        // ServiceLocator.Get<PlayerScript>().TakeDamage(0, Vector3.right);
    }

    public void PlaySwishSfx() {
    }
    
    public void PlayShotSfx() {
        FMODManager.PlaySfx("Misc/Spell_blast");
    }

    public void BreakFloor() {
        floorAnimator.Play(floorAnimation.name);
    }
}