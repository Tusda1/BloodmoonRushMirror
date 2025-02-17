using System;
using UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "DialogueCharacter Says",
        story: "[dialogueCharacter] says [dialogue]",
        category: "Action",
        id: "2f66c0875cb50dfa9564f10bbb4723ec")]
    public class DialogueCharacterSaysAction : Action {
        #region ---Fields---
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO> dialogueCharacter;
        [SerializeReference] public BlackboardVariable<string> dialogue;
        
        [SerializeReference] public BlackboardVariable<string> characterNameOverride;
        [SerializeReference] public BlackboardVariable<Sprite> imagePortraitOverride;
        [SerializeReference] public BlackboardVariable<FontAsset> dialogueFontOverride;
        [SerializeReference] public BlackboardVariable<float> textSizeOverride;
        [SerializeReference] public BlackboardVariable<Color> dialogueColorOverride;
        [SerializeReference] public BlackboardVariable<float> textSpeedOverride = new(0);
        [SerializeReference] public BlackboardVariable<bool> overwriteWhenIsSfxPlayed = new(false);
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO.WhenIsSfxPlayed> whenIsSfxPlayed;

        private int _currentCharIndex;
        private float _timer;
        private bool _skipped;
        private bool _textDoneDisplaying;
        private float _timePerChar;
        private string _displayText = "";

        private static DialogueUI DialogueUI => DialogueUI.Instance;

        private DialogueCharacterSO.WhenIsSfxPlayed _whenIsSfxPlayed;

        private DialogueCharacterSO DialogueCharacter => dialogueCharacter.Value;
        #endregion

        protected override Status OnStart() {
            if (FailureCondition()) return Status.Failure;

            SetDialogueUIValues(DialogueUI);

            _currentCharIndex = -1;
            _skipped = false;
            _displayText = dialogue.Value;
            _textDoneDisplaying = false;

            try {
                _whenIsSfxPlayed = DialogueCharacter.whenIsSfxPlayed;

                if (overwriteWhenIsSfxPlayed.Value) {
                    _whenIsSfxPlayed = whenIsSfxPlayed.Value;
                }
            }
            catch (Exception) {
                // ignored
            }

            if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.InTheBeginning) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere ||
                (_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerWord) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                PlayTextAudio();
            }

            return Status.Running;
        }

        protected override Status OnUpdate() {
            if (_skipped) {
                if (_textDoneDisplaying) {
                    return Status.Success;
                }

                DialogueUI.dialogueText = dialogue.Value;
                _textDoneDisplaying = true;
                _skipped = false;

                return Status.Running;
            }

            if (_textDoneDisplaying) {
                return Status.Running;
            }

            _timer += Time.deltaTime;

            if (_timer >= _timePerChar) {
                _timer = 0f;
                _currentCharIndex++;

                try {
                    if (char.IsWhiteSpace(_displayText[_currentCharIndex])) {
                        _currentCharIndex++;
                        if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerWord) !=
                            DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                            PlayTextAudio();
                        }
                    }
                }
                catch (Exception) {
                    // ignored
                }

                _currentCharIndex = Math.Clamp(_currentCharIndex, 0, _displayText.Length);
                DialogueUI.dialogueText = _displayText[.._currentCharIndex];

                if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerCharacter) !=
                    DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                    PlayTextAudio();
                }

                if (_currentCharIndex >= _displayText.Length) {
                    DialogueUI.dialogueText = _displayText;
                    _textDoneDisplaying = true;
                }
            }

            return Status.Running;
        }

        protected override void OnEnd() {
            DialogueUI.OnSkipDialogue -= Skip;
        }

        private void Skip() {
            _skipped = true;
        }

        private void PlayTextAudio() {
            DialogueUI.AudioManager.PlayOneShot(DialogueCharacter.talkSoundID);
        }

        private bool FailureCondition() {
            if (dialogueCharacter == null) {
                Debug.LogWarning("DialogueCharacter not set!");
                return true;
            }

            if (DialogueUI == null) {
                Debug.LogWarning("DialogueUI not set!");
                return true;
            }

            return false;
        }

        private void SetDialogueUIValues(DialogueUI dialogueUI) {
            dialogueUI.OnSkipDialogue += Skip;

            dialogueUI.dialogueFont = dialogueFontOverride.Value != null
                ? dialogueFontOverride.Value
                : DialogueCharacter.dialogueFont;

            dialogueUI.imagePortrait = imagePortraitOverride.Value != null
                ? imagePortraitOverride.Value
                : DialogueCharacter.dialogueImage;

            dialogueUI.dialogueColor = dialogueColorOverride.Value != default
                ? dialogueColorOverride.Value
                : DialogueCharacter.dialogueColor;

            dialogueUI.textSize = !Mathf.Approximately(textSizeOverride.Value, default)
                ? textSizeOverride.Value
                : DialogueCharacter.textSize;

            _timePerChar = 1f / (textSpeedOverride.Value != 0
                ? textSpeedOverride.Value
                : DialogueCharacter.textSpeed);

            dialogueUI.characterName = characterNameOverride.Value != ""
                ? characterNameOverride.Value 
                : DialogueCharacter.characterName;

            dialogueUI.dialogueText = "";
        }
    }
}