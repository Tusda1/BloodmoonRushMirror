using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(AudioDictionarySO),
    menuName = "Scriptable Objects/Create " + nameof(AudioDictionarySO))]
public class AudioDictionarySO : ScriptableObject {
    [SerializedDictionary("SoundID", "Sound Path")]
    public SerializedDictionary<string, string> dictionary;
}