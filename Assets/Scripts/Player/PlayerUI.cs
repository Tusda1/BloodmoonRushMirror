using Attributes;
using UIToolkitTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Player {
    public class PlayerUI : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        private VisualElement _root;
        private PlayerScript _player;
        private HeartBar _playerHeartBar;
        private HeartBar _playerTempHeartBar;
        #endregion

        private void Start() {
            _player = ServiceLocator.Get<PlayerScript>();

            _root = uiDocument.rootVisualElement;
            _root.dataSource = _player;

            _playerHeartBar = _root.Q<HeartBar>("PlayerHeartBar");
            _playerTempHeartBar = _root.Q<HeartBar>("PlayerTempHeartBar");

            _player.healthComponent.OnHealthChanged += health =>
                OnHealthOnOnHealthChanged(health, _player.healthComponent.TempHealth);

            OnHealthOnOnHealthChanged(_player.healthComponent.Health, _player.healthComponent.TempHealth);
        }

        private void OnHealthOnOnHealthChanged(int health, int tempHealth) {
            _playerHeartBar.heartCount = health;
            _playerTempHeartBar.heartCount = tempHealth;
        }
    }
}