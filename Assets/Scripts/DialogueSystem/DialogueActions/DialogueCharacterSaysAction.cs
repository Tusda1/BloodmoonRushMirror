using System;
using FMODScripts;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "DialogueCharacter Says",
        story: "[dialogueCharacter] says [dialogue]",
        category: "Action",
        id: "2f66c0875cb50dfa9564f10bbb4723ec")]
    public class DialogueCharacterSaysAction : Action {
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO> dialogueCharacter;
        [SerializeReference] public BlackboardVariable<string> dialogue;

        [SerializeReference] public BlackboardVariable<Sprite> imagePortraitOverride;
        [SerializeReference] public BlackboardVariable<Font> dialogueFontOverride;
        [SerializeReference] public BlackboardVariable<float> textSizeOverride;
        [SerializeReference] public BlackboardVariable<Color> dialogueColorOverride;
        [SerializeReference] public BlackboardVariable<float> textSpeedOverride;
        [SerializeReference] public BlackboardVariable<bool> overwriteWhenIsSfxPlayed;
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO.WhenIsSfxPlayed> whenIsSfxPlayed;

        private int _currentCharIndex;
        private float _timer;
        private bool _skipped;
        private bool _textDoneDisplaying;
        private float _timePerChar;
        private string _displayText;
        
        private static DialogueUI DialogueUI => DialogueUI.Instance;
        private static FMODManager FMODManager => FMODManager.Instance;
        
        private DialogueCharacterSO.WhenIsSfxPlayed _whenIsSfxPlayed;

        protected override Status OnStart() {
            #region ---FailureConditions---

            if (dialogueCharacter == null) {
                Debug.LogWarning("DialogueCharacter not set!");
                return Status.Failure;
            }

            if (DialogueUI == null) {
                Debug.LogWarning("DialogueUI not set!");
                return Status.Failure;
            }

            #endregion

            DialogueUI.OnSkipDialogue += Skip;

            DialogueUI.dialogueFont = dialogueFontOverride.Value == null
                ? dialogueCharacter.Value.dialogueFont
                : dialogueFontOverride.Value;
            
            DialogueUI.imagePortrait = imagePortraitOverride.Value == null
                ? dialogueCharacter.Value.dialogueImage
                : imagePortraitOverride.Value;
            
            DialogueUI.dialogueColor = dialogueColorOverride.Value == default
                ? dialogueCharacter.Value.dialogueColor
                : dialogueColorOverride.Value;
            
            DialogueUI.textSize = textSizeOverride.Value == 0 ? dialogueCharacter.Value.textSize : textSizeOverride.Value;
            
            DialogueUI.dialogueText = "";
            
            _timePerChar = 1f / (textSpeedOverride == 0 ? dialogueCharacter.Value.textSpeed : textSpeedOverride.Value);

            _currentCharIndex = -1;
            _skipped = false;
            _displayText = dialogue.Value;
            _textDoneDisplaying = false;

            _whenIsSfxPlayed = overwriteWhenIsSfxPlayed ? whenIsSfxPlayed.Value : dialogueCharacter.Value.whenIsSfxPlayed;

            if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.InTheBeginning) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere ||
                (_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerWord) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                FMODManager.PlayOneShot(dialogueCharacter.Value.textAudioSoundEventPath);
            }

            return Status.Running;
        }

        protected override Status OnUpdate() {
            if (_skipped) {
                if (_textDoneDisplaying) {
                    return Status.Success;
                }

                Debug.Log("Skipped");
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
                            FMODManager.PlayOneShot(dialogueCharacter.Value.textAudioSoundEventPath);
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
                    FMODManager.PlayOneShot(dialogueCharacter.Value.textAudioSoundEventPath);
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
    }
}