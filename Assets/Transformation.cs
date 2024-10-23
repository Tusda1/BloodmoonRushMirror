using Player;
using UnityEngine;

public class Transformation : StateMachineBehaviour {
    public PlayerHitbox hitbox;
    public DamageInstance damageInstance;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        hitbox.damageInstance = damageInstance;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    }
}