using UIToolkitTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Player {
    public class PlayerUI : MonoBehaviour {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private PlayerScript _player;
        private HeartBar _playerHeartBar;
        private HeartBar _playerTempHeartBar;

        private void Start() {
            _player = ServiceLocator.Get<PlayerScript>();

            _root = uiDocument.rootVisualElement;
            _root.dataSource = _player;

            _playerHeartBar = _root.Q<HeartBar>("PlayerHeartBar");
            _playerTempHeartBar = _root.Q<HeartBar>("PlayerTempHeartBar");

            _player.health.OnHealthChanged += health => OnHealthOnOnHealthChanged(health, _player.health.TempHealth);

            OnHealthOnOnHealthChanged(_player.health.Health, _player.health.TempHealth);
        }

        private void OnHealthOnOnHealthChanged(int health, int tempHealth) {
            _playerHeartBar.heartCount = health;
            _playerTempHeartBar.heartCount = tempHealth;
        }
    }
}