using SceneCollections;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(SaveSlotData), 
    menuName = "SaveLoadSystem/UITK Create " + nameof(SaveSlotData))]
public class SaveSlotData : ScriptableObject {    
    public string saveSlotName;
    public string lastTimePlayed;
    public string currentLevel;
    public string currentProgress;
    
    public SceneCollectionSO currentSceneCollection;
}