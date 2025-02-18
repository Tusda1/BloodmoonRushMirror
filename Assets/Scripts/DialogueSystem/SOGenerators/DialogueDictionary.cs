using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(DialogueDictionary), 
    menuName = "Scriptable Objects/Create " + nameof(DialogueDictionary))]
public class DialogueDictionary : ScriptableObject {
    [SerializedDictionary("TextID", "Dialogue Dictionary Content")]
    public SerializedDictionary<string, DialogueDictionaryContent> dialogueDictionary = new();

    [Serializable]
    public class DialogueDictionaryContent {
        public DialogueCharacterSO dialogueCharacterSO;
        public string text;
    }
}