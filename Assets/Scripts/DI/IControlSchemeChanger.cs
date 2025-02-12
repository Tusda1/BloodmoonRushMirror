using System;
using UnityEngine.InputSystem;

public interface IControlSchemeChanger {
    string CurrentControlSchemeName { get; set; }
    string CurrentControlSchemeID { get; set; }
    
    bool ControlSchemeIsController { get; }
    
    ControlSchemeDataSO CurrentControlSchemeDataSO { get; }

    ControlSchemeData CurrentControlSchemeData { get; }
    
    event Action<ControlSchemeData> OnControlSchemeChanged;
    
    void ChangeSchemeByName(string scheme);
    void ChangeSchemeByID(string scheme);
    
    public struct ControlSchemeData {
        public string Name;
        public string ID;
        public ControlSchemeDataSO DataSO;
        public InputDevice[] Devices;
        
        public ControlSchemeData(ControlSchemeDataSO dataSO, InputDevice[] devices, string name = null, string id = null) {
            Name = name ?? dataSO.controlScheme;
            ID = id ?? dataSO.id;
            DataSO = dataSO;
            Devices = devices;
        }
    }
}