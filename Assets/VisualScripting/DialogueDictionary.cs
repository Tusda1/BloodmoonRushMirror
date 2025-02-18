using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(DialogueDictionary), 
    menuName = "Scriptable Objects/Create " + nameof(DialogueDictionary))]
public class DialogueDictionary : ScriptableObject {
    public SerializedDictionary<string, string> dialogueDictionary = new();
}