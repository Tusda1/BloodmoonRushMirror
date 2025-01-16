using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement]
    public partial class AnimatedSpriteElement : VisualElement {
        #region ---Fields---
        [UxmlAttribute] public Sprite Sprite {
            set {
                _sprite = value;
                
                if (Sprite == null) {
                    Debug.LogWarning("Sprite is not assigned.");
                    return;
                }
            
                style.backgroundImage = new StyleBackground(Sprite);
                style.width = new StyleLength(Sprite.rect.width);
                style.height = new StyleLength(Sprite.rect.height);
            }
            get => _sprite;
        }
        private Sprite _sprite;

        private Sprite[] AnimationFrames => new[] { AnimationFrame1, AnimationFrame2, AnimationFrame3, AnimationFrame4 };
        
        [UxmlAttribute] public Sprite AnimationFrame1;
        [UxmlAttribute] public Sprite AnimationFrame2;
        [UxmlAttribute] public Sprite AnimationFrame3;
        [UxmlAttribute] public Sprite AnimationFrame4;
        [UxmlAttribute] public float delayPerFrame;
        
        [UxmlAttribute] public bool BeginAnimation {
            set {
                _beginAnimation = value;
                if (_beginAnimation) {
                    _ = AnimateSprite();
                }
                else {
                    _cancellationTokenSource?.Cancel();
                }
            }
            get => _beginAnimation;
        }
        private bool _beginAnimation;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        #endregion

        private async Awaitable AnimateSprite() {
            if (_isRunning) return;
            
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var progress = 0;

            try {
                while (true) {
                    Sprite = AnimationFrames[progress];
                    await Awaitable.WaitForSecondsAsync(delayPerFrame, _cancellationTokenSource.Token);
                    progress++;
                    if (progress >= AnimationFrames.Length) {
                        progress = 0;
                    }
                }
            }
            catch (OperationCanceledException) {
            }
            finally {
                _isRunning = false;
                Sprite = _sprite;
                _cancellationTokenSource.Dispose();
            }
        }
    }
}