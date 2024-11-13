using UnityEngine;

public class AnimationEventStateBehaviour : StateMachineBehaviour {
    public string eventName;
    [Range(0f, 1f)] public float triggerTime;

    private bool _hasTriggered;
    private AnimationEventReceiver _receiver;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        _hasTriggered = false;
        _receiver = animator.GetComponent<AnimationEventReceiver>();
        
        if (triggerTime == 0) {
            NotifyReceiver();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (triggerTime is 0 or 1) return;
        
        var currentTime = stateInfo.normalizedTime % 1f;

        if (!_hasTriggered && currentTime >= triggerTime) {
            NotifyReceiver();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (triggerTime >= 1) {
            NotifyReceiver();
        }
    }

    private void NotifyReceiver() {
        if (_receiver != null) {
            _receiver.OnAnimationEventTriggered(eventName);
        }
        
        _hasTriggered = true;
    }
}
