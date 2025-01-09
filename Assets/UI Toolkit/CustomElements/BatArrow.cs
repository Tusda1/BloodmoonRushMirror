using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement]
    public partial class BatArrow : VisualElement {
        #region ---Fields---
        [UxmlAttribute] public Sprite UnselectedSprite {
            set {
                _unselectedSprite = value;
                SetUnselected();
            }
            get => _unselectedSprite;
        }
        private Sprite _unselectedSprite;
        
        [UxmlAttribute] public Sprite SelectedSprite {
            set {
                _selectedSprite = value;
                SetSelected();
            }
            get => _selectedSprite;
        }
        private Sprite _selectedSprite;
        
        public event Action OnClick = delegate { };
        #endregion

        public BatArrow() {
            RegisterCallback<ClickEvent>(_ => OnClick.Invoke());
        }

        public void Enable(Action onClickAction) {
            Debug.Log("Enable: " + name);
            OnClick = onClickAction;
            focusable = true;
            style.opacity = new StyleFloat(1);
            enabledSelf = true;
        }

        public void Disable() {
            OnClick = delegate { };
            focusable = false;
            style.opacity = new StyleFloat(0.5f);
            enabledSelf = false;
            SetUnselected();
        }

        public void SetUnselected() {
            if (UnselectedSprite == null) {
                Debug.LogWarning("Unselected sprite is not assigned.");
                return;
            }
            
            style.backgroundImage = new StyleBackground(UnselectedSprite);
            style.width = new StyleLength(UnselectedSprite.rect.width);
            style.height = new StyleLength(UnselectedSprite.rect.height);
        }
        
        public void SetSelected() {
            if (SelectedSprite == null) {
                Debug.LogWarning("Selected sprite is not assigned.");
                return;
            }
            
            style.backgroundImage = new StyleBackground(SelectedSprite);
            style.width = new StyleLength(SelectedSprite.rect.width);
            style.height = new StyleLength(SelectedSprite.rect.height);
        }

        public void TriggerOnClick() {
            OnClick.Invoke();
        }
    }
}