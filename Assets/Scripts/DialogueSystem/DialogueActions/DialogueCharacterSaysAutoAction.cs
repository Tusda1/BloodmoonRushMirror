using System;
using UI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "DialogueCharacterSaysAuto",
        story: "[dialogueCharacterReference] Says [dialogue] | Auto |",
        category: "DialogueAction",
        id: "0466ecf5f83b7ef25e26fd10319bf1e2")]
    public class DialogueCharacterSaysAutoAction : Action {
        [SerializeReference] public BlackboardVariable<DialogueCharacter> dialogueCharacterReference;
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
        private float _timePerChar;

        private string _displayText;
        private DialogueUI dialogueUI => DialogueUI.Instance;
        private DialogueCharacter dialogueCharacter => dialogueCharacterReference.Value;
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

            dialogueUI.dialogueFont = dialogueFontOverride.Value == null
                ? dialogueCharacter.dialogueFont
                : dialogueFontOverride.Value;
            dialogueUI.imagePortrait = imagePortraitOverride.Value == null
                ? dialogueCharacter.dialogueImage
                : imagePortraitOverride.Value;
            dialogueUI.dialogueColor = dialogueColorOverride.Value == default
                ? dialogueCharacter.dialogueColor
                : dialogueColorOverride.Value;
            dialogueUI.textSize = textSizeOverride.Value == 0 ? dialogueCharacter.textSize : textSizeOverride.Value;
            dialogueUI.dialogueText = "";
            _timePerChar = 1f / (textSpeedOverride == 0 ? dialogueCharacter.textSpeed : textSpeedOverride.Value);

            _currentCharIndex = -1;
            _displayText = dialogue.Value;

            _whenIsSfxPlayed = overwriteWhenIsSfxPlayed ? whenIsSfxPlayed.Value : dialogueCharacter.whenIsSfxPlayed;

            if ((_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.InTheBeginning) !=
                DialogueCharacter.WhenIsSfxPlayed.Nowhere ||
                (_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.PerWord) !=
                DialogueCharacter.WhenIsSfxPlayed.Nowhere) {
                FmodManager.Instance.PlayOneShot(dialogueCharacter.textAudioSoundEventPath);
            }

            return Status.Running;
        }

        protected override Status OnUpdate() {
            _timer += Time.deltaTime;

            if (_timer >= _timePerChar) {
                _timer = 0f;
                _currentCharIndex++;

                try {
                    if (char.IsWhiteSpace(_displayText[_currentCharIndex])) {
                        _currentCharIndex++;
                        if ((_whenIsSfxPlayed & DialogueCharacter.WhenIsSfxPlayed.PerWord) !=
                            DialogueCharacter.WhenIsSfxPlayed.Nowhere) {
                            FmodManager.Instance.PlayOneShot(dialogueCharacter.textAudioSoundEventPath);
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
                    FmodManager.Instance.PlayOneShot(dialogueCharacter.textAudioSoundEventPath);
                }

                if (_currentCharIndex >= _displayText.Length) {
                    dialogueUI.dialogueText = _displayText;
                    return Status.Success;
                }
            }

            return Status.Running;
        }
    }
}