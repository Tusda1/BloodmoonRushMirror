using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement]
    public partial class SpriteElement : VisualElement {
        #region ---Fields---
        [UxmlAttribute] public Sprite Sprite {
            set {
                _sprite = value;
                SetSprite();
            }
            get => _sprite;
        }
        private Sprite _sprite;
        #endregion

        public void SetSprite() {
            if (Sprite == null) {
                // Debug.LogWarning("Sprite is not assigned.");
                return;
            }
            
            style.backgroundImage = new StyleBackground(Sprite);
            style.width = new StyleLength(Sprite.rect.width);
            style.height = new StyleLength(Sprite.rect.height);
        }
    }
}