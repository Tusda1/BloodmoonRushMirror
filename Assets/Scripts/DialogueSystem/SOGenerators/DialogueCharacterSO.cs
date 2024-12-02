using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = nameof(DialogueCharacterSO), menuName = "Behavior/Create " + nameof(DialogueCharacterSO))]
public class DialogueCharacterSO : ScriptableObject {
    public Sprite dialogueImage;
    public string characterName;
    public FontAsset dialogueFont;
    public float textSize = 8;
    public float textSpeed = 16;
    public Color dialogueColor = Color.black;
    public string textAudioSoundEventPath = "Talk/";
    public WhenIsSfxPlayed whenIsSfxPlayed;

    [Serializable, Flags] public enum WhenIsSfxPlayed : byte {
        Nowhere = 0,
        InTheBeginning = 1 << 0,
        PerWord = 1 << 1,
        PerCharacter = 1 << 2,
    }
}