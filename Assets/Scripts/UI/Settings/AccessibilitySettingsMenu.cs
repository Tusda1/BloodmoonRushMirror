using System;
using Attributes;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class AccessibilitySettingsMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField] private FontAsset singleFont;

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };

        #region ***---PrivateFields---***
        private VisualElement _root;
        private VisualElement _accessibilityMenu;
        #endregion
        
        public static bool UseSingleFont;
        public static FontAsset SingleFont;
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _accessibilityMenu = _root.Q<VisualElement>("AccessibilityMenu");
        }

        private void Start() {
            SingleFont = singleFont;
            var singleFontToggle = _root.Q<Toggle>("SingleFont");
            singleFontToggle.RegisterValueChangedCallback(evt => {
                UseSingleFont = evt.newValue;
                PlayerPrefs.SetInt("UseSingleFont", UseSingleFont ? 1 : 0);
            });
            singleFontToggle.value = PlayerPrefs.GetInt("UseSingleFont", 0) == 1;
        }
        #endregion

        #region ---IMenu---
        public void Activate() {
            _accessibilityMenu.SetEnabled(true);
            _accessibilityMenu.RemoveFromClassList("left");
            _accessibilityMenu.RemoveFromClassList("right");

            OnMenuActivated.Invoke();
        }

        public void Deactivate(string addToClassList = null) {
            _accessibilityMenu.SetEnabled(false);

            if (addToClassList != null) {
                _accessibilityMenu.AddToClassList(addToClassList);
            }

            OnMenuDeactivated.Invoke();
        }
        #endregion
    }
}