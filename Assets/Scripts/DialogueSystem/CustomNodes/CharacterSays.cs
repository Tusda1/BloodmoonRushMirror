using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static DialogueCharacterSO;

[UnitShortTitle("Character Says")] [UnitCategory("DialogueUI")] [TypeIcon(typeof(BoltAnimationEvent))]
public class CharacterSays : Unit {
    #region ---Fields---
    #region ***---Control_IO---***
    [DoNotSerialize] [PortLabelHidden] public ControlInput Enter;
    [DoNotSerialize] [PortLabelHidden] public ControlOutput Exit;
    #endregion

    private const float AutoContinueExitDelay = 0.5f;
    private string _sfxSoundID;
    private IAudioManager _audioManager;

    #region ***---Input---***
    public ValueInput DialogueDictionary;
    public ValueInput DialogueUIDataContainer;
    public ValueInput AudioManager;
    public ValueInput AutoContinue;
    public ValueInput TextID;
    #endregion

    #region ***----Output---***
    [DoNotSerialize] [PortLabelHidden] public ValueOutput DialogueDictionary_Output;
    [DoNotSerialize] [PortLabelHidden] public ValueOutput DialogueUIDataContainer_Output;
    [DoNotSerialize] [PortLabelHidden] public ValueOutput AudioManager_Output;
    [DoNotSerialize] [PortLabelHidden] public ValueOutput AutoContinue_Output;
    #endregion

    #region ***---OverrideValues---***
    [Inspectable] public Sprite CharacterPortraitOverride;
    [Inspectable] public FontAsset TextFontAssetOverride;
    [Inspectable] public string CharacterNameOverride;
    [Inspectable] public FontAsset CharacterNameFontAssetOverride;
    [Inspectable] public float TextSizeOverride;
    [Inspectable] public float TextSpeedOverride;
    [Inspectable] public Color TextColorOverride;
    [Inspectable] public string TextOverride;
    [Inspectable] public string TalkSoundIDOverride;
    [Inspectable] public bool OverrideWhenSFXIsPlayed;
    [Inspectable] public WhenIsSfxPlayed WhenIsSFXPlayedOverride;
    #endregion

    private static CancellationTokenSource _cancellationTokenSource;

    private static event Action TypewriterOnBegin = delegate { };
    private static event Action TypewriterOnNewWord = delegate { };
    private static event Action TypewriterOnNewCharacter = delegate { };
    #endregion

    protected override void Definition() {
        Enter = ControlInputCoroutine(nameof(Enter), Action);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);

        DialogueDictionary = ValueInput<DialogueDictionary>(nameof(DialogueDictionary), null);
        DialogueUIDataContainer = ValueInput<DialogueUI_DataContainer>(nameof(DialogueUIDataContainer), null);
        AudioManager = ValueInput<IAudioManager>(nameof(AudioManager), null);
        AutoContinue = ValueInput(nameof(AutoContinue), false);
        TextID = ValueInput(nameof(TextID), string.Empty);

        DialogueDictionary_Output = ValueOutput(nameof(DialogueDictionary),
            flow => flow.GetValue<DialogueDictionary>(DialogueDictionary));

        DialogueUIDataContainer_Output = ValueOutput(nameof(DialogueUIDataContainer),
            flow => flow.GetValue<DialogueUI_DataContainer>(DialogueUIDataContainer));

        AudioManager_Output = ValueOutput(nameof(AudioManager),
            flow => flow.GetValue<IAudioManager>(AudioManager));

        AutoContinue_Output = ValueOutput(nameof(AutoContinue),
            flow => flow.GetValue<bool>(AutoContinue));
    }

    private IEnumerator Action(Flow flow) {
        var dialogueDictionary = flow.GetValue<DialogueDictionary>(DialogueDictionary);
        var dialogueUIDataContainer = flow.GetValue<DialogueUI_DataContainer>(DialogueUIDataContainer);
        _audioManager = flow.GetValue<IAudioManager>(AudioManager);
        var autoContinue = flow.GetValue<bool>(AutoContinue);
        var textID = flow.GetValue<string>(TextID);
        var dialogueCharacter = TextIDToCharacter(dialogueDictionary, textID);
        var end = false;
        _cancellationTokenSource = new CancellationTokenSource();

        Initialize();

        var fullText = TextOverride == string.Empty ? TextOverride : TextIDToText(dialogueDictionary, textID);

        PlayerScript.SkipDialogueEvent += SkipTypewriter;

        var typewriter = Typewriter(dialogueUIDataContainer, fullText);

        yield return new WaitUntil(() => typewriter.IsCompleted);

        PlayerScript.SkipDialogueEvent -= SkipTypewriter;

        if (autoContinue) {
            yield return new WaitForSeconds(AutoContinueExitDelay);
        }
        else {
            PlayerScript.SkipDialogueEvent += End;

            yield return new WaitUntil(() => end);

            PlayerScript.SkipDialogueEvent -= End;
        }

        ClearSoundToTypewriterEvents();

        yield return Exit;

        yield break;

        void Initialize() {
            dialogueUIDataContainer.characterPortrait = CharacterPortraitOverride ?? dialogueCharacter.dialogueImage;
            
            dialogueUIDataContainer.characterNameFontAsset = CharacterNameFontAssetOverride ?? dialogueCharacter.dialogueFont;
            
            dialogueUIDataContainer.characterName = string.IsNullOrEmpty(CharacterNameOverride)
                ? dialogueCharacter.characterName
                : CharacterNameOverride;

            dialogueUIDataContainer.textSize = TextSizeOverride == 0
                ? dialogueCharacter.textSize
                : TextSizeOverride;
            
            dialogueUIDataContainer.textSpeed = TextSpeedOverride == 0
                ? dialogueCharacter.textSpeed
                : TextSpeedOverride;
            
            dialogueUIDataContainer.textColor = TextColorOverride == default
                ? dialogueCharacter.dialogueColor
                : TextColorOverride;
            
            dialogueUIDataContainer.textFontAsset = TextFontAssetOverride ?? dialogueCharacter.dialogueFont;

            _sfxSoundID = string.IsNullOrEmpty(TalkSoundIDOverride)
                ? dialogueCharacter.talkSoundID
                : TalkSoundIDOverride;

            ApplySoundToTypewriterEvents(OverrideWhenSFXIsPlayed
                ? WhenIsSFXPlayedOverride
                : dialogueCharacter.whenIsSFXPlayed);

            dialogueUIDataContainer.text = "";
        }
        
        void End() {
            end = true;
        }
    }

    private void ApplySoundToTypewriterEvents(WhenIsSfxPlayed whenIsSfxPlayed) {
        if ((whenIsSfxPlayed & WhenIsSfxPlayed.InTheBeginning) != WhenIsSfxPlayed.Nowhere) {
            TypewriterOnBegin += PlaySFX;
        }

        if ((whenIsSfxPlayed & WhenIsSfxPlayed.PerCharacter) != WhenIsSfxPlayed.Nowhere) {
            TypewriterOnNewWord += PlaySFX;
        }

        if ((whenIsSfxPlayed & WhenIsSfxPlayed.PerCharacter) != WhenIsSfxPlayed.Nowhere) {
            TypewriterOnNewCharacter += PlaySFX;
        }
    }
    
    private void ClearSoundToTypewriterEvents() {
        TypewriterOnBegin -= PlaySFX;
        TypewriterOnNewWord -= PlaySFX;
        TypewriterOnNewCharacter -= PlaySFX;
    }

    private void PlaySFX() {
        _audioManager.PlayOneShot(_sfxSoundID);
    }

    private static void SkipTypewriter() {
        _cancellationTokenSource.Cancel();
    }

    private static async Awaitable Typewriter(DialogueUI_DataContainer dialogueUIDataContainer, string fulltext) {
        try {
            TypewriterOnBegin.Invoke();
            TypewriterOnNewWord.Invoke();

            var textDelay = 1 / dialogueUIDataContainer.textSpeed;
            
            foreach (var c in fulltext) {
                dialogueUIDataContainer.text += c;
                if (char.IsWhiteSpace(c)) {
                    TypewriterOnNewWord.Invoke();
                    continue;
                }

                TypewriterOnNewCharacter.Invoke();
                await Awaitable.WaitForSecondsAsync(textDelay, _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException) {
            dialogueUIDataContainer.text = fulltext;
        }
    }

    private static string TextIDToText(DialogueDictionary dialogueDictionary, string textID) {
#if UNITY_EDITOR
        try {
#endif
            return dialogueDictionary.dialogueDictionary[textID].text;
#if UNITY_EDITOR
        }
        catch (KeyNotFoundException) {
            throw new KeyNotFoundException($"ID: '{textID}' not found in AudioDictionary!");
        }
#endif
    }

    private static DialogueCharacterSO TextIDToCharacter(DialogueDictionary dialogueDictionary, string textID) {
#if UNITY_EDITOR
        try {
#endif
            return dialogueDictionary.dialogueDictionary[textID].dialogueCharacterSO;
#if UNITY_EDITOR
        }
        catch (KeyNotFoundException) {
            throw new KeyNotFoundException($"ID: '{textID}' not found in AudioDictionary!");
        }
#endif
    }
}