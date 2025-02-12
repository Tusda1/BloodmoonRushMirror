using System;
using Attributes;
using Consensus;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        [HideInInspector] public ScriptVotePair scriptVotePair = new();
        public event Action OnSkipDialogue = delegate { };
        
        [Inject] public IAudioManager AudioManager;
        #endregion

        protected override void Awake() {
            base.Awake();

            _root = uiDocument.rootVisualElement;
            _root.dataSource = this;
        }

        private void OnEnable() {
            playerCanMove.ChangeVote(scriptVotePair, true);
        }

        private void OnDisable() {
            playerCanMove.DeleteVote(scriptVotePair);
        }

        public void TriggerOnSkipDialogue() {
            OnSkipDialogue.Invoke();
        }
    }
}