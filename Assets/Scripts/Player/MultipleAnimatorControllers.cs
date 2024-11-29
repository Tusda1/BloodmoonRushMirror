using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;

namespace Player {
    [Serializable] public class MultipleAnimatorControllers {
        public Animator[] animators;
            
        public void SetBool(int id, bool value) {
            foreach (var animator in animators) {
                animator.SetBool(id, value);
            }
        }

        public void SetFloat(int id, float value) {
            foreach (var animator in animators) {
                animator.SetFloat(id, value);
            }
        }

        public void SetTrigger(int id) {
            foreach (var animator in animators) {
                animator.SetTrigger(id);
            }
        }

        public void ResetTrigger(int id) {
            foreach (var animator in animators) {
                animator.ResetTrigger(id);
            }
        }

        public AnimationClip GetCurrentAnimationClip() {
            return animators.First().GetCurrentAnimationClip();
        }

        public IEnumerable<T> GetBehaviours<T>() where T : StateMachineBehaviour {
            return animators.SelectMany(animator => animator.GetBehaviours<T>());
        }
    }
}