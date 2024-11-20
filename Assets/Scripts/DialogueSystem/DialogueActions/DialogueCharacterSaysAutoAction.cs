using System;
using FMODScripts;
using UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "DialogueCharacter Says Auto",
        story: "[dialogueCharacter] says [dialogue] | Auto |",
        category: "DialogueAction",
        id: "0466ecf5f83b7ef25e26fd10319bf1e2")]
    public class DialogueCharacterSaysAutoAction : Action {
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO> dialogueCharacter;
        [SerializeReference] public BlackboardVariable<string> dialogue;

        [SerializeReference] public BlackboardVariable<Sprite> imagePortraitOverride;
        [SerializeReference] public BlackboardVariable<FontAsset> dialogueFontOverride;
        [SerializeReference] public BlackboardVariable<float> textSizeOverride;
        [SerializeReference] public BlackboardVariable<Color> dialogueColorOverride;
        [SerializeReference] public BlackboardVariable<float> textSpeedOverride;
        [SerializeReference] public BlackboardVariable<bool> overwriteWhenIsSfxPlayed;
        [SerializeReference] public BlackboardVariable<DialogueCharacterSO.WhenIsSfxPlayed> whenIsSfxPlayed;

        private int _currentCharIndex;
        private float _timer;
        private float _timePerChar;

        private string _displayText;
        private static DialogueUI DialogueUI => DialogueUI.Instance;
        private static FMODManager FMODManager => FMODManager.Instance;
        private DialogueCharacterSO DialogueCharacterSO => dialogueCharacter.Value;

        private DialogueCharacterSO.WhenIsSfxPlayed _whenIsSfxPlayed;

        protected override Status OnStart() {
            #region ---FailureConditions---
            if (DialogueCharacterSO == null) {
                Debug.LogWarning("DialogueCharacter not set!");
                return Status.Failure;
            }

            if (DialogueUI == null) {
                Debug.LogWarning("DialogueUI not set!");
                return Status.Failure;
            }
            #endregion

            DialogueUI.dialogueFont = dialogueFontOverride.Value == null
                ? DialogueCharacterSO.dialogueFont
                : dialogueFontOverride.Value;
            
            DialogueUI.imagePortrait = imagePortraitOverride.Value == null
                ? DialogueCharacterSO.dialogueImage
                : imagePortraitOverride.Value;
            
            DialogueUI.dialogueColor = dialogueColorOverride.Value == default
                ? DialogueCharacterSO.dialogueColor
                : dialogueColorOverride.Value;
            
            DialogueUI.textSize = textSizeOverride.Value == 0 ? DialogueCharacterSO.textSize : textSizeOverride.Value;
            
            DialogueUI.dialogueText = "";
            
            _timePerChar = 1f / (textSpeedOverride == 0 ? DialogueCharacterSO.textSpeed : textSpeedOverride.Value);

            _currentCharIndex = -1;
            _displayText = dialogue.Value;

            _whenIsSfxPlayed = overwriteWhenIsSfxPlayed 
                ? whenIsSfxPlayed.Value 
                : DialogueCharacterSO.whenIsSfxPlayed;

            if ((_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.InTheBeginning) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere ||
                (_whenIsSfxPlayed & DialogueCharacterSO.WhenIsSfxPlayed.PerWord) !=
                DialogueCharacterSO.WhenIsSfxPlayed.Nowhere) {
                FMODManager.PlayOneShot(DialogueCharacterSO.textAudioSoundEventPath);
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
                            FMODManager.PlaySfx(DialogueCharacterSO.textAudioSoundEventPath);
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
                    FMODManager.PlaySfx(DialogueCharacterSO.textAudioSoundEventPath);
                }

                if (_currentCharIndex >= _displayText.Length) {
                    DialogueUI.dialogueText = _displayText;
                    return Status.Success;
                }
            }

            return Status.Running;
        }
    }
}