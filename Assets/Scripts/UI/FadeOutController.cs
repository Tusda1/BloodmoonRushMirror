using System.Collections.Generic;
using Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    public class FadeOutController : Singleton<FadeOutController> {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        private VisualElement _root;
        private VisualElement _overlay;
        #endregion

        private void Start() {
            _root = uiDocument.rootVisualElement;
            _overlay = _root.Q<VisualElement>("Overlay");
        }

        public void SetAnimationTime(float time) {
            var t = new List<TimeValue> { new(time, TimeUnit.Second) };
            _overlay.style.transitionDuration = new StyleList<TimeValue>(t);
        }

        public void FadeOut() {
            _overlay.RemoveFromClassList("opacity0");
        }

        public void FadeIn() {
            _overlay.AddToClassList("opacity0");
        }
    }
}