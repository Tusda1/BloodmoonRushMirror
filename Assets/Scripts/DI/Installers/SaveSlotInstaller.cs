using UnityEngine;
using Zenject;

[CreateAssetMenu(
    fileName = nameof(SaveSlotInstaller), 
    menuName = "Installers/" + nameof(SaveSlotInstaller))]
public class SaveSlotInstaller : ScriptableObjectInstaller<SaveSlotInstaller> {
    [SerializeField] private SaveSlotDataSO saveSlot1Data;
    [SerializeField] private SaveSlotDataSO saveSlot2Data;
    [SerializeField] private SaveSlotDataSO saveSlot3Data;
    
    public override void InstallBindings() {
        Container.BindInstance(saveSlot1Data).WithId("Slot1");
        Container.BindInstance(saveSlot2Data).WithId("Slot2");
        Container.BindInstance(saveSlot3Data).WithId("Slot3");
    }
}