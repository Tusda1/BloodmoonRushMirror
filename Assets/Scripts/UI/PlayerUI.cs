using Attributes;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Player {
    public class PlayerUI : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField] private DialogueUI_DataContainer dialogueUI;

        private VisualElement _root;
        private PlayerHearts _playerHearts;
        
        private PlayerScript _playerScript;
        #endregion

        [Inject]
        public void Construct(IObjectResolver container) {
            _playerScript = container.Resolve<PlayerScript>();
        }

        private void Start() {
            _root = uiDocument.rootVisualElement;

            dialogueUI.playerUIDocumentVisible = DisplayStyle.Flex;

            _playerHearts = _root.Q<PlayerHearts>("PlayerHearts");
            
            _root.dataSource = _playerScript;

            _playerScript.healthComponent.OnHealthChanged += health =>
                OnHealthOnOnHealthChanged(health, _playerScript.healthComponent.TempHealth);

            _playerHearts.HeartContainerCount = _playerScript.healthComponent.maxHealth;
            OnHealthOnOnHealthChanged(_playerScript.healthComponent.Health, _playerScript.healthComponent.TempHealth);
        }

        private void OnHealthOnOnHealthChanged(int health, int tempHealth) {
            _playerHearts.DefaultHeartCount = health;
            _playerHearts.TempHeartCount = tempHealth;
        }
    }
}