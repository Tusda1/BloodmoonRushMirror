using System;
using Fmod;
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
        [SerializeReference] public BlackboardVariable<DialogueCharacter> dialogueCharacter;
        [SerializeReference] public BlackboardVariable<string> dialogue;

        [SerializeReference] public BlackboardVariable<Sprite> imagePortraitOverride;
        [SerializeReference] public BlackboardVariable<Font> dialogueFontOverride;
        [SerializeReference] public BlackboardVariable<float> textSizeOverride;
        [SerializeReference] public BlackboardVariable<Color> dialogueColorOverride;
        [SerializeReference] public BlackboardVariable<float> textSpeedOverride;
        [SerializeReference] public BlackboardVariable<bool> overwriteWhenIsSfxPlayed;
        [SerializeReference] public BlackboardVariable<DialogueCharacter.WhenIsSfxPlayed> whenIsSfxPlayed;

        private int _currentCharIndex;
        private float _timer;
        private bool _skipped;
        private bool _textDoneDisplaying;
        private float _timePerChar;
        private string _displayText;
        
        private DialogueUI dialogueUI => DialogueUI.Instance;
        private FmodManager fmodManager => FmodManager.Instance;
        private DialogueCharacter.WhenIsSfxPlayed _whenIsSfxPlayed;

        protected override Status OnStart() {
            #region ---FailureConditions---

            if (dialogueCharacter == null) {
                Debug.LogWarning("DialogueCharacter not set!");
                return Status.Failure;
            }

            if (dialogueUI == null) {
                Debug.LogWarning("DialogueUI not set!");
                return Status.Failure;
            }

            #endregion

            dialogueUI.OnSkipDialogue += Skip;

            dialogueUI.dialogueFont = dialogueFontOverride.Value == null
                ? dialogueCharacter.Value.dialogueFont
                : dialogueFontOverride.Value;
            
            dialogueUI.imagePortrait = imagePortraitOverride.Value == null
                ? dialogueCharacter.Value.dialogueImage
                : imagePortraitOverride.Value;
            
            dialogueUI.dialogueColor = dialogueColorOverride.Value == default
                ? dialogueCharacter.Value.dialogueColor
                : dialogueColorOverride.Value;
            
            dialogueUI.textSize = textSizeOverride.Value == 0 ? dialogueCharacter.Value.textSize : textSizeOverride.Value;
            
            dialogueUI.dialogueText = "";
            
            _timePerChar = 1f / (textSpeedOverride == 0 ? dialogueCharacter.Value.textSpeed : textSpeedOverride.Value);

            _currentCharIndex = -1;
            _skipped = false;
            _displayText = dialogue.Value;
            _textDoneDisplaying = false;

            _whenIsSfxPlayed = overwriteWhenIsSfxPlayed ? whenIsSfxPlayed.Value : dialogueCharacter.Value.whenIsSfxPlayed;

            if ((_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.InTheBeginning) !=
                DialogueCharacter.WhenIsSfxPlayed.Nowhere ||
                (_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.PerWord) !=
                DialogueCharacter.WhenIsSfxPlayed.Nowhere) {
                fmodManager.PlayOneShot(dialogueCharacter.Value.textAudioSoundEventPath);
            }

            return Status.Running;
        }

        protected override Status OnUpdate() {
            if (_skipped) {
                if (_textDoneDisplaying) {
                    return Status.Success;
                }

                Debug.Log("Skipped");
                dialogueUI.dialogueText = dialogue.Value;
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
                        if ((_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.PerWord) !=
                            DialogueCharacter.WhenIsSfxPlayed.Nowhere) {
                            fmodManager.PlayOneShot(dialogueCharacter.Value.textAudioSoundEventPath);
                        }
                    }
                }
                catch (Exception) {
                    // ignored
                }

                _currentCharIndex = Math.Clamp(_currentCharIndex, 0, _displayText.Length);
                dialogueUI.dialogueText = _displayText[.._currentCharIndex];

                if ((_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.PerCharacter) !=
                    DialogueCharacter.WhenIsSfxPlayed.Nowhere) {
                    fmodManager.PlayOneShot(dialogueCharacter.Value.textAudioSoundEventPath);
                }

                if (_currentCharIndex >= _displayText.Length) {
                    dialogueUI.dialogueText = _displayText;
                    _textDoneDisplaying = true;
                }
            }

            return Status.Running;
        }

        protected override void OnEnd() {
            dialogueUI.OnSkipDialogue -= Skip;
        }

        private void Skip() {
            _skipped = true;
        }
    }
}