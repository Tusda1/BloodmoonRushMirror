using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "Behavior/new DialogueCharacter")]
public class DialogueCharacter : ScriptableObject {
    public Sprite dialogueImage;
    public Font dialogueFont;
    public float textSize = 8;
    public float textSpeed = 16;
    public Color dialogueColor = Color.black;
    public string textAudioSoundEventPath = "event:/Sfx/Talk/";
    public WhenIsSfxPlayed whenIsSfxPlayed;

    [Serializable, Flags] public enum WhenIsSfxPlayed {
        Nowhere = 0,
        InTheBeginning = 1 << 0,
        PerWord = 1 << 1,
        PerCharacter = 1 << 2,
    }
}