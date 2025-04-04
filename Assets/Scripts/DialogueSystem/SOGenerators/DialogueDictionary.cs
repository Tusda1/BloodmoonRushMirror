using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using static DialogueCharacterSO;
#if UNITY_EDITOR
using DialogueSystem;
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = nameof(DialogueDictionary),
    menuName = "Scriptable Objects/Create " + nameof(DialogueDictionary))]
public class DialogueDictionary : ScriptableObject {
    [SerializedDictionary("TextID", "Dialogue Dictionary Content")]
    public SerializedDictionary<string, DialogueDictionaryValue> dialogueDictionary = new();
#if UNITY_EDITOR
    [ContextMenu(nameof(SetDataFromCSVFile))]
    public void SetDataFromCSVFile() {
        var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!assetPath.EndsWith(".csv")) return;
        
        CSVConverter.CSVToSO(assetPath, this);
    }
#endif
    [Serializable] public class DialogueDictionaryValue {
        public string text;
        public DialogueCharacterSO character;
        public WhenIsSfxPlayed whereToPlaySFX;
        public bool autoContinue;
        public float textSpeed;
        public string soundID;
        public string characterName;

        public DialogueDictionaryValue(
            DialogueCharacterSO character,
            string text,
            WhenIsSfxPlayed whereToPlaySFX,
            bool autoContinue,
            float textSpeed, 
            string soundID,
            string characterName) {
            this.character = character;
            this.text = text;
            this.whereToPlaySFX = whereToPlaySFX;
            this.autoContinue = autoContinue;
            this.textSpeed = textSpeed;
            this.soundID = soundID;
            this.characterName = characterName;
        }
    }
}