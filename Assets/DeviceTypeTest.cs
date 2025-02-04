using TMPro;
using UnityEngine;
using VContainer;
using static IControlSchemeChanger;

public class DeviceTypeTest : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private TextMeshPro textMesh;

    private IControlSchemeChanger _controlSchemeChanger;
    #endregion

    [Inject]
    public void Construct(IControlSchemeChanger controlSchemeChanger) {
        _controlSchemeChanger = controlSchemeChanger;
    }

    #region ---EventFunctions---
    private void Start() {
        _controlSchemeChanger.OnControlSchemeChanged += OnControlSchemeChanged;
        textMesh.text = _controlSchemeChanger.CurrentControlSchemeID;
    }

    private void OnDestroy() {
        _controlSchemeChanger.OnControlSchemeChanged -= OnControlSchemeChanged;
    }
    #endregion

    private void OnControlSchemeChanged(ControlSchemeData controlSchemeData) {
        textMesh.text = controlSchemeData.ID;
    }
}