using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(AudioDictionarySO),
    menuName = "Scriptable Objects/Create " + nameof(AudioDictionarySO))]
public class AudioDictionarySO : ScriptableObject {
    public SerializedDictionary<string, string> dictionary;
}