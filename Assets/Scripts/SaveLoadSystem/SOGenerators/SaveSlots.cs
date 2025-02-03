using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(SaveSlots), 
    menuName = "Scriptable Objects/" + nameof(SaveSlots))]
public class SaveSlots : ScriptableObject {
    public SaveSlotDataSO saveSlot1Data;
    public SaveSlotDataSO saveSlot2Data;
    public SaveSlotDataSO saveSlot3Data;
}