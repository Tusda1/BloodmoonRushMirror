using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement] public partial class PlayerHearts : VisualElement {
        #region ---Fields---
        #region ***---HeartContainer---***
        [UxmlAttribute]
        public int HeartContainerCount {
            get => _heartContainerCount;
            set {
                _heartContainerCount = math.clamp(value, 0, 20);
                UpdateHeartsContainers(HeartContainerCount);
            }
        }

        [UxmlAttribute] private Sprite HeartContainer { get; set; }

        private int _heartContainerCount;
        #endregion

        #region ***---DefaultHeart---***
        [UxmlAttribute]
        public int DefaultHeartCount {
            get => _defaultHeartCount;
            set {
                _defaultHeartCount = math.clamp(value, 0, HeartContainerCount);
                UpdateDefaultHearts(DefaultHeartCount);
            }
        }
        
        [UxmlAttribute] private Sprite DefaultHeart { get; set; }

        private int _defaultHeartCount;
        private List<VisualElement> _defaultHearts = new();
        #endregion
        
        #region ***---TempHeart---***
        [UxmlAttribute]
        public int TempHeartCount {
            get => _tempHeartCount;
            set {
                _tempHeartCount = math.clamp(value, 0, DefaultHeartCount);
                UpdateTempHearts(TempHeartCount);
            }
        }
        
        [UxmlAttribute] private Sprite TempHeart { get; set; }

        private int _tempHeartCount;
        private List<VisualElement> _tempHearts = new();
        #endregion

        private List<VisualElement> Hearts {
            get {
                if (_hearts == null) {
                    return _hearts;
                }

                return Children().ToList();
            }
            set => _hearts = value;
        }

        private List<VisualElement> _hearts = new();
        #endregion

        private void UpdateHeartsContainers(int hearts) {
            if (HeartContainer == null || DefaultHeart == null || TempHeart == null) return;

            Clear();
            Hearts.Clear();
            _defaultHearts.Clear();
            _tempHearts.Clear();

            for (var i = 0; i < hearts; i++) {
                var element = CreateElementFromTemplate(HeartContainer);
                
                var defaultHeart = CreateElementFromTemplate(DefaultHeart, element);
                defaultHeart.style.marginLeft = new StyleLength(1);
                defaultHeart.style.marginTop = new StyleLength(1);
                defaultHeart.style.display = DisplayStyle.None;
                defaultHeart.name = "DefaultHeart";
                _defaultHearts.Add(defaultHeart);
                
                var tempHeart = CreateElementFromTemplate(TempHeart, defaultHeart);
                tempHeart.style.display = DisplayStyle.None;
                tempHeart.name = "TempHeart";
                _tempHearts.Add(tempHeart);
                
                Hearts.Add(element);
            }
        }

        private void UpdateDefaultHearts(int heartContainerCount) {
            if (DefaultHeart == null || HeartContainer == null || heartContainerCount == 0) return;
            
            for (var i = 0; i < Hearts.Count; i++) {
                if (i >= heartContainerCount) {
                    _defaultHearts[i].style.display = DisplayStyle.None;
                    continue;
                }
                
                _defaultHearts[i].style.display = DisplayStyle.Flex;
            }
        }
        
        private void UpdateTempHearts(int tempHeartCount) {
            if (TempHeart == null || HeartContainer == null || DefaultHeart == null || tempHeartCount == 0) return;
            
            for (var i = 0; i < Hearts.Count; i++) {
                if (i >= tempHeartCount) {
                    _tempHearts[i].style.display = DisplayStyle.None;
                    continue;
                }
        
                _tempHearts[i].style.display = DisplayStyle.Flex;
            }
        }

        private VisualElement CreateElementFromTemplate(Sprite sprite, VisualElement parentElement = null) {
            parentElement ??= this;
            
            var element = new SpriteElement {
                Sprite = sprite
            };
            
            parentElement.Add(element);
            
            return element;
        }
    }
}