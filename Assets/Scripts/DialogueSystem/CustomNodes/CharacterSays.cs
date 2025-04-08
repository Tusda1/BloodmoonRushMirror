using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Player;
using UI.Settings;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static DialogueCharacterSO;

#pragma warning disable UDR0001
#pragma warning disable UDR0005

[UnitShortTitle("Character Says")] 
[UnitCategory("DialogueUI")] 
[TypeIcon(typeof(BoltAnimationEvent))]
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
    public ValueInput TextID;
    #endregion

    #region ***----Output---***
    [DoNotSerialize] [PortLabelHidden] public ValueOutput DialogueDictionary_Output;
    [DoNotSerialize] [PortLabelHidden] public ValueOutput DialogueUIDataContainer_Output;
    [DoNotSerialize] [PortLabelHidden] public ValueOutput AudioManager_Output;
    #endregion

    #region ***---OverrideValues---***
    [Inspectable] public Sprite CharacterPortraitOverride;
    [Inspectable] public FontAsset CharacterNameFontAssetOverride;
    [Inspectable] public float CharacterNameSizeOverride;
    [Inspectable] public Color CharacterNameColorOverride;
    
    [Inspectable] public FontAsset TextFontAssetOverride;
    [Inspectable] public float TextSizeOverride;
    [Inspectable] public Color TextColorOverride;
    #endregion

    private static CancellationTokenSource _typewriterTask;

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
        TextID = ValueInput(nameof(TextID), string.Empty);

        DialogueDictionary_Output = ValueOutput(nameof(DialogueDictionary),
            flow => flow.GetValue<DialogueDictionary>(DialogueDictionary));

        DialogueUIDataContainer_Output = ValueOutput(nameof(DialogueUIDataContainer),
            flow => flow.GetValue<DialogueUI_DataContainer>(DialogueUIDataContainer));

        AudioManager_Output = ValueOutput(nameof(AudioManager),
            flow => flow.GetValue<IAudioManager>(AudioManager));
    }
    
    private IEnumerator Action(Flow flow) {
        var dialogueDictionary = flow.GetValue<DialogueDictionary>(DialogueDictionary);
        var dialogueUIDataContainer = flow.GetValue<DialogueUI_DataContainer>(DialogueUIDataContainer);
        _audioManager = flow.GetValue<IAudioManager>(AudioManager);
        var textID = flow.GetValue<string>(TextID);
        var dialogueCharacter = TextIDToCharacter(dialogueDictionary, textID);
        var end = false;
        _typewriterTask = new CancellationTokenSource();

        var dialogueValues = dialogueDictionary.dialogueDictionary[textID];

        Initialize();

        var fullText = TextIDToText(dialogueDictionary, textID);

        PlayerScript.SkipDialogueEvent += SkipTypewriter;

        var typewriter = Typewriter(dialogueUIDataContainer, fullText);

        yield return new WaitUntil(() => typewriter.IsCompleted);

        PlayerScript.SkipDialogueEvent -= SkipTypewriter;

        if (dialogueValues.autoContinue) {
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
            #region ***---Character---***
            dialogueUIDataContainer.characterPortrait = CharacterPortraitOverride ?? dialogueCharacter.dialogueImage;
            
            dialogueUIDataContainer.characterNameFontAsset = CharacterNameFontAssetOverride ?? dialogueCharacter.dialogueFont;
            
            dialogueUIDataContainer.characterName = dialogueValues.characterName;

            dialogueUIDataContainer.characterNameSize = CharacterNameSizeOverride == 0
                ? dialogueCharacter.textSize
                : CharacterNameSizeOverride;
            
            dialogueUIDataContainer.textColor = CharacterNameColorOverride == default
                ? dialogueCharacter.dialogueColor
                : CharacterNameColorOverride;
            #endregion
            
            #region ***---Text---***
            dialogueUIDataContainer.textSize = TextSizeOverride == 0
                ? dialogueCharacter.textSize
                : TextSizeOverride;

            dialogueUIDataContainer.textSpeed = dialogueValues.textSpeed;
            
            dialogueUIDataContainer.textColor = TextColorOverride == default
                ? dialogueCharacter.dialogueColor
                : TextColorOverride;
            
            dialogueUIDataContainer.textFontAsset = SettingsMenu.UseSingleFont ? 
                SettingsMenu.SingleFont
                : TextFontAssetOverride ?? dialogueCharacter.dialogueFont;

            _sfxSoundID = dialogueValues.soundID;

            ApplySoundToTypewriterEvents(dialogueValues.whereToPlaySFX);

            dialogueUIDataContainer.text = "";
            #endregion
        }
        
        void End() {
            end = true;
        }
    }
    
    private void ApplySoundToTypewriterEvents(WhenIsSfxPlayed whenIsSfxPlayed) {
        if ((whenIsSfxPlayed & WhenIsSfxPlayed.InTheBeginning) != WhenIsSfxPlayed.Nowhere) {
            TypewriterOnBegin += PlaySFX;
        }

        if ((whenIsSfxPlayed & WhenIsSfxPlayed.PerWord) != WhenIsSfxPlayed.Nowhere) {
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
        _typewriterTask.Cancel();
    }

    private static async Awaitable Typewriter(DialogueUI_DataContainer dialogueUIDataContainer, string fulltext) {
        try {
            TypewriterOnBegin.Invoke();
            TypewriterOnNewWord.Invoke();

            var textDelay = 1 / dialogueUIDataContainer.textSpeed;
            
            var richTextTag = false;
            
            foreach (var c in fulltext) {
                dialogueUIDataContainer.text += c;

                if (c == '<') richTextTag = true;

                if (richTextTag) {
                    if (c == '>') richTextTag = false;
                    
                    continue;
                }
                
                if (char.IsWhiteSpace(c)) {
                    TypewriterOnNewWord.Invoke();
                    continue;
                }

                TypewriterOnNewCharacter.Invoke();
                await Awaitable.WaitForSecondsAsync(textDelay, _typewriterTask.Token);
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
            return dialogueDictionary.dialogueDictionary[textID].character;
#if UNITY_EDITOR
        }
        catch (KeyNotFoundException) {
            throw new KeyNotFoundException($"ID: '{textID}' not found in AudioDictionary!");
        }
#endif
    }
}
#pragma warning restore UDR0001
#pragma warning restore UDR0005