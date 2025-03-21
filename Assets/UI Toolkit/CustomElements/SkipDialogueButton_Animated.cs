using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement]
    public partial class SkipDialogueButton_Animated : VisualElement {
        #region ---Fields---
        [UxmlAttribute] public Sprite Sprite {
            set {
                _sprite = value;
                
                if (Sprite == null) {
                    // Debug.LogWarning("Sprite is not assigned.");
                    return;
                }
            
                style.backgroundImage = new StyleBackground(Sprite);
                style.width = new StyleLength(Sprite.rect.width);
                style.height = new StyleLength(Sprite.rect.height);
            }
            get => _sprite;
        }
        private Sprite _sprite;

        public SpriteAnimation SpriteAnimation;
        #endregion

        public async Awaitable Animate() {
            var progress = 0;

            while (true) {
                Sprite = SpriteAnimation.frames[progress];
                await Awaitable.WaitForSecondsAsync(SpriteAnimation.secondsPerFrame);
                progress++;
                if (progress >= SpriteAnimation.frames.Length) {
                    progress = 0;
                }
            }
        }
    }
}