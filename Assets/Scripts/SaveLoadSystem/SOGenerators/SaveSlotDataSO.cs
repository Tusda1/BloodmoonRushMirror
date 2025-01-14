using SceneCollections;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(SaveSlotDataSO), 
    menuName = "SaveLoadSystem/UITK Create " + nameof(SaveSlotDataSO))]
public class SaveSlotDataSO : ScriptableObject {    
    public string saveSlotName;
    public string lastTimePlayed;
    public string currentLevel;
    public string currentProgress;
    
    public SceneCollectionSO currentSceneCollection;

    public void Clear() {
        lastTimePlayed = "";
        currentLevel = "";
        currentProgress = "";
        currentSceneCollection = null;
    }
}