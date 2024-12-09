using Player;
using UnityEngine;

public class PlayerAttackBehaviour : StateMachineBehaviour {
    public PlayerHitbox hitbox;
    public DamageInstance damageInstance;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        hitbox = ServiceLocator.Get<PlayerHitbox>();
        damageInstance.OnHitAction += hitbox.TriggerOnHitAction;
        damageInstance.OnKillAction += hitbox.TriggerOnKillAction;
        hitbox.damageInstance = damageInstance;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    }
}