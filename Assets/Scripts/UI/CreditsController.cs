using Attributes;
using UnityEngine;
using UnityEngine.UIElements;

public class CreditsController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
    
    private VisualElement _root;
    #endregion
    
    private void Start() {
        _root = uiDocument.rootVisualElement;
    }
}