using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement]
    public partial class BatArrow : Button {
        private bool _isEnabled;
        
        public event Action OnClick = delegate { };

        public void SetOnClickAction(Action onClick) {
            OnClick = onClick;
        }

        public void Enable() {
            if (_isEnabled) return;
            
            Debug.Log("Enable: " + name);
            focusable = true;
            style.opacity = new StyleFloat(1);
            enabledSelf = true;
            clicked += OnClick.Invoke;
            _isEnabled = true;
        }

        public void Disable() {
            if (!_isEnabled) return;
            
            focusable = false;
            style.opacity = new StyleFloat(0.25f);
            enabledSelf = false;
            clicked -= OnClick.Invoke;
            _isEnabled = false;
        }

        public void TriggerOnClick() {
            if (!_isEnabled) return;
            
            OnClick.Invoke();
        }
    }
}