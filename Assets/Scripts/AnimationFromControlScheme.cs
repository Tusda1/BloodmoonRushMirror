using Attributes;
using UnityEngine;
using VContainer;
using UnityEngine.U2D.Animation;

public class AnimationFromControlScheme : MonoBehaviour {
    #region ---Fields---
    #region ***---GetOrAddComponents---***
    [GetOrAddComponent, SerializeField, HideInInspector] private SpriteLibrary spriteLibrary;
    #endregion

    private IControlSchemeChanger _controlSchemeChanger;
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _controlSchemeChanger = container.Resolve<IControlSchemeChanger>();
    }
    #endregion

    #region ---EventFunctions---
    private void Start() {
        _controlSchemeChanger.OnControlSchemeChanged += OnControlSchemeChanged;
        OnControlSchemeChanged(_controlSchemeChanger.CurrentControlSchemeData);
    }

    private void OnDestroy() {
        _controlSchemeChanger.OnControlSchemeChanged -= OnControlSchemeChanged;
    }
    #endregion

    private void OnControlSchemeChanged(IControlSchemeChanger.ControlSchemeData data) {
        spriteLibrary.spriteLibraryAsset = data.DataSO.spriteLibrary;
    }
}