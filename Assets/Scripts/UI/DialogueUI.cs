using System;
using Attributes;
using Consensus;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using VContainer;

namespace UI {
    public class DialogueUI : Singleton<DialogueUI> {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        #region ****---Inspector---***
        [SerializeField] public AllInFavourConsensusSO playerCanMove;

        [Header("DialogueUI Settings")]
        public bool uiDocumentVisible;
        public Sprite imagePortrait;
        public string characterName;
        public string dialogueText;
        public FontAsset dialogueFont;
        public float textSize;
        public Color dialogueColor;
        #endregion

        private VisualElement _root;
        private SkipDialogueButton_Animated _skipDialogueButton;

        [HideInInspector] public ScriptVotePair scriptVotePair = new();
        public event Action OnSkipDialogue = delegate { };
        
        public IAudioManager AudioManager;
        [HideInInspector] public CameraEffects cameraEffects;
        private IControlSchemeChanger _controlSchemeChanger;
        
        private Awaitable _animationTask;
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            AudioManager = container.Resolve<IAudioManager>();
            cameraEffects = container.Resolve<CameraEffects>();
            _controlSchemeChanger = container.Resolve<IControlSchemeChanger>();
        }

        protected override void Awake() {
            base.Awake();

            _root = uiDocument.rootVisualElement;
            _root.dataSource = this;
            _skipDialogueButton = _root.Q<SkipDialogueButton_Animated>();
        }

        private void Start() {
            playerCanMove.ChangeVote(scriptVotePair, true);
            
            DisableDialogueUI();
        }
        #endregion
        
        public void EnableDialogueUI() {
            _controlSchemeChanger.OnControlSchemeChanged += OnControlSchemeChanged;
            OnControlSchemeChanged(_controlSchemeChanger.CurrentControlSchemeData);
            _animationTask = _skipDialogueButton.Animate();
            _root.style.display = DisplayStyle.Flex;
            uiDocumentVisible = true;
        }

        public void DisableDialogueUI() {
            playerCanMove.DeleteVote(scriptVotePair);
            
            _controlSchemeChanger.OnControlSchemeChanged -= OnControlSchemeChanged;
            _animationTask?.Cancel();
            _root.style.display = DisplayStyle.None;
            uiDocumentVisible = false;
        }

        private void OnControlSchemeChanged(IControlSchemeChanger.ControlSchemeData data) {
            _skipDialogueButton.SpriteAnimation = data.DataSO.skipDialogueButtonAnimation;
        }

        public void TriggerOnSkipDialogue() {
            OnSkipDialogue.Invoke();
        }

        public void Initialize() {
            
        }

        public async Awaitable Typewriter() {
            // write l

            Awaitable.WaitForSecondsAsync(5);
        }
    }
}