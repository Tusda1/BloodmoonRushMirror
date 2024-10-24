using Gaskellgames;
using Player;
using UnityEngine;

public class PlayerAttackBehaviour : StateMachineBehaviour {
    [ReadOnly] public PlayerHitbox hitbox;
    public DamageInstance damageInstance;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        damageInstance.OnHitAction += hitbox.TriggerOnHitAction;
        damageInstance.OnKillAction += hitbox.TriggerOnKillAction;
        hitbox.damageInstance = damageInstance;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    }
}