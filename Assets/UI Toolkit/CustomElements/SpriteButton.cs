using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement]
    public partial class SpriteButton : Button {
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
                Debug.LogWarning("Sprite is not assigned.");
                return;
            }
            
            style.backgroundImage = new StyleBackground(Sprite);
            style.width = new StyleLength(Sprite.rect.width);
            style.height = new StyleLength(Sprite.rect.height);
            
            style.paddingBottom = 0;
            style.paddingTop = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            
            style.marginBottom = 0;
            style.marginTop = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
        }
    }
}