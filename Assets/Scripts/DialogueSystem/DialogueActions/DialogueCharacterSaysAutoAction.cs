using System;
using Fmod;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "DialogueCharacter Says Auto",
        story: "[dialogueCharacterReference] says [dialogue] | Auto |",
        category: "DialogueAction",
        id: "0466ecf5f83b7ef25e26fd10319bf1e2")]
    public class DialogueCharacterSaysAutoAction : Action {
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO> dialogueCharacterReference;
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
        private float _timePerChar;

        private string _displayText;
        private DialogueUI dialogueUI => DialogueUI.Instance;
        private FmodManager fmodManager => FmodManager.Instance;
        private DialogueCharacterSO dialogueCharacterSO => dialogueCharacterReference.Value;
        private DialogueCharacterSO.WhenIsSfxPlayed _whenIsSfxPlayed;

        protected override Status OnStart() {
            #region ---FailureConditions---

            if (dialogueCharacterSO == null) {
                Debug.LogWarning("DialogueCharacter not set!");
                return Status.Failure;
            }

            if (dialogueUI == null) {
                Debug.LogWarning("DialogueUI not set!");
                return Status.Failure;
            }

            #endregion

            dialogueUI.dialogueFont = dialogueFontOverride.Value == null
                ? dialogueCharacterSO.dialogueFont
                : dialogueFontOverride.Value;
            dialogueUI.imagePortrait = imagePortraitOverride.Value == null
                ? dialogueCharacterSO.dialogueImage
                : imagePortraitOverride.Value;
            dialogueUI.dialogueColor = dialogueColorOverride.Value == default
                ? dialogueCharacterSO.dialogueColor
                : dialogueColorOverride.Value;
            dialogueUI.textSize = textSizeOverride.Value == 0 ? dialogueCharacterSO.textSize : textSizeOverride.Value;
            dialogueUI.dialogueText = "";
            _timePerChar = 1f / (textSpeedOverride == 0 ? dialogueCharacterSO.textSpeed : textSpeedOverride.Value);

            _currentCharIndex = -1;
            _displayText = dialogue.Value;

            _whenIsSfxPlayed = overwriteWhenIsSfxPlayed ? whenIsSfxPlayed.Value : dialogueCharacterSO.whenIsSfxPlayed;

            if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.InTheBeginning) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere ||
                (_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerWord) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                fmodManager.PlayOneShot(dialogueCharacterSO.textAudioSoundEventPath);
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
                        if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerWord) !=
                            DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                            fmodManager.PlayOneShot(dialogueCharacterSO.textAudioSoundEventPath);
                        }
                    }
                }
                catch (Exception) {
                    // ignored
                }

                _currentCharIndex = Math.Clamp(_currentCharIndex, 0, _displayText.Length);
                dialogueUI.dialogueText = _displayText[.._currentCharIndex];

                if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerCharacter) !=
                    DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                    fmodManager.PlayOneShot(dialogueCharacterSO.textAudioSoundEventPath);
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