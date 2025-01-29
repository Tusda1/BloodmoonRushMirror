using Attributes;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Player {
    public class PlayerUI : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        private VisualElement _root;
        private PlayerHearts _playerHearts;
        
        [Inject] private PlayerScript _player;
        #endregion

        private void Start() {
            _root = uiDocument.rootVisualElement;

            _playerHearts = _root.Q<PlayerHearts>("PlayerHearts");
            
            _root.dataSource = _player;

            _player.healthComponent.OnHealthChanged += health =>
                OnHealthOnOnHealthChanged(health, _player.healthComponent.TempHealth);

            _playerHearts.HeartContainerCount = _player.healthComponent.maxHealth;
            OnHealthOnOnHealthChanged(_player.healthComponent.Health, _player.healthComponent.TempHealth);
        }

        private void OnHealthOnOnHealthChanged(int health, int tempHealth) {
            _playerHearts.DefaultHeartCount = health;
            _playerHearts.TempHeartCount = tempHealth;
        }
    }
}