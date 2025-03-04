using Attributes;
using Consensus;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI {
    public class DialogueUI : Singleton<DialogueUI> {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField] private DialogueUI_DataContainer dialogueUIDataContainer;

        private VisualElement _root;
        private SkipDialogueButton_Animated _skipDialogueButton;
        
        private float _textSpeed;

        private IControlSchemeChanger _controlSchemeChanger;
        
        private Awaitable _animationTask;
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            _controlSchemeChanger = container.Resolve<IControlSchemeChanger>();
        }

        protected override void Awake() {
            base.Awake();

            _root = uiDocument.rootVisualElement;
            _skipDialogueButton = _root.Q<SkipDialogueButton_Animated>();
        }

        private void Start() {
            dialogueUIDataContainer.uiDocumentVisible = DisplayStyle.None;
            _controlSchemeChanger.OnControlSchemeChanged += OnControlSchemeChanged;
            OnControlSchemeChanged(_controlSchemeChanger.CurrentControlSchemeData);
            _animationTask = _skipDialogueButton.Animate();
        }

        protected override void OnDestroy() {
            _controlSchemeChanger.OnControlSchemeChanged -= OnControlSchemeChanged;
            _animationTask?.Cancel();
        }
        #endregion

        private void OnControlSchemeChanged(IControlSchemeChanger.ControlSchemeData data) {
            _skipDialogueButton.SpriteAnimation = data.DataSO.skipDialogueButtonAnimation;
        }
    }
}