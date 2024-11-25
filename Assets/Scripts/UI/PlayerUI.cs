using Attributes;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Player {
    public class PlayerUI : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        private PlayerScript _player;
        private VisualElement _root;
        private Hearts _playerHeartBar;
        private Hearts _playerTempHeartBar;
        #endregion

        private void Start() {
            _player = ServiceLocator.Get<PlayerScript>();

            _root = uiDocument.rootVisualElement;
            _root.dataSource = _player;

            _playerHeartBar = _root.Q<Hearts>("PlayerHearts");
            _playerTempHeartBar = _root.Q<Hearts>("PlayerTempHearts");

            _player.healthComponent.OnHealthChanged += health =>
                OnHealthOnOnHealthChanged(health, _player.healthComponent.TempHealth);

            OnHealthOnOnHealthChanged(_player.healthComponent.Health, _player.healthComponent.TempHealth);
        }

        private void OnHealthOnOnHealthChanged(int health, int tempHealth) {
            _playerHeartBar.HeartCount = health;
            _playerTempHeartBar.HeartCount = tempHealth;
        }
    }
}