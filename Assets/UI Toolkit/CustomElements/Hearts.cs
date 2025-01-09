using UnityEngine.UIElements;

namespace UI_Toolkit {
    [UxmlElement] public partial class Hearts : VisualElement {
        #region ---Fields---
        [UxmlAttribute]
        public int HeartCount {
            get => _heartCount;
            set {
                _heartCount = value;
                UpdateHearts(_heartCount);
            }
        }

        [UxmlAttribute]
        private VisualTreeAsset HeartTemplates {
            get => _heartTemplates;
            set {
                _heartTemplates = value;
                UpdateHearts(HeartCount);
            }
        }

        private int _heartCount;
        private VisualTreeAsset _heartTemplates;
        #endregion

        private void UpdateHearts(int hearts) {
            if (_heartTemplates == null) return;

            Clear();

            for (var i = 0; i < hearts; i++) {
                CreateHeart(HeartTemplates);
            }
        }

        private void CreateHeart(VisualTreeAsset heartTemplate) {
            var element = new VisualElement();
            heartTemplate.CloneTree(element);
            Add(element);
        }
    }
}