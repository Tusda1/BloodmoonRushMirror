using DG.Tweening;
using Fmod;
using Player;
using Unity.Behavior;
using UnityEngine;

public class Boss : MonoBehaviour, IDamageable {
    [SerializeField] private StartNewConversationEventChannel startNewConversationEvent;
    [SerializeField] private BehaviorGraph bossDialogue;
    [SerializeField] private ActionEventChannel actionEvent;
    
    [SerializeField] private Animator floorAnimator;
    [SerializeField] private AnimationClip floorAnimation;

    private FmodManager fmodManager => FmodManager.Instance;
    
    private Animator _animator;

    private void Start() {
        _animator = GetComponent<Animator>();
        
        transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 0.5f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public bool TakeDamage(int damageAmount, Vector3 attackDirection = default) {
        var dialogueVariables = bossDialogue.BlackboardReference.Blackboard.Variables;
        dialogueVariables.Find(x => x.Type == typeof(Boss)).ObjectValue = this;
        dialogueVariables.Find(x => x.Type == typeof(Animator)).ObjectValue = _animator;
        startNewConversationEvent.SendEventMessage(bossDialogue);
        DOTween.Kill(transform);
        
        ServiceLocator.Get<PlayerScript>().TakeDamage(0, Vector3.right);
        
        return false;   
    }

    public void PlaySwishSfx() {
        // FmodManager.Instance.PlayOneShot();
    }
    
    public void PlayShotSfx() {
        fmodManager.PlayOneShot("event:/Sfx/Misc/Spell_blast");
    }

    public void BreakFloor() {
        floorAnimator.Play(floorAnimation.name);
    }

    // Do not remove will cause an error for no reason
    public void Test() {
    }
}