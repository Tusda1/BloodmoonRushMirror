using System;
using UnityEngine.InputSystem;

public interface IControlSchemeChanger {
    string CurrentControlSchemeName { get; set; }
    string CurrentControlSchemeID { get; set; }
    
    InputLocalizationData CurrentControlSchemeData { get; }

    ControlSchemeData CurrentControlSchemeInfo { get; }
    
    event Action<ControlSchemeData> OnControlSchemeChanged;
    
    void ChangeSchemeByName(string scheme);
    void ChangeSchemeByID(string scheme);
    
    public struct ControlSchemeData {
        public string Name;
        public string ID;
        public InputLocalizationData Data;
        public InputDevice[] Devices;
        
        public ControlSchemeData(InputLocalizationData data, InputDevice[] devices, string name = null, string id = null) {
            Name = name ?? data.controlScheme;
            ID = id ?? data.id;
            Data = data;
            Devices = devices;
        }
    }
}