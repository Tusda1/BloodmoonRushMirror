using System.Collections.Generic;
using UnityEngine;

public class InputLocalizationData : ScriptableObject {
    
}

public class InputLocalizationProvider : IProvider<InputLocalizationData> {
    private Dictionary<string, InputLocalizationData> _services;

    Dictionary<string, InputLocalizationData> IProvider<InputLocalizationData>.Services => _services;
}