using Attributes;
using UnityEngine;
using VContainer;
using ExtensionMethods;

public class AnimationFromControlScheme : MonoBehaviour {
    #region ---Fields---
    #region ***---GetOrAddComponents---***
    [GetOrAddComponent, SerializeField, HideInInspector] private SpriteRenderer spriteRenderer;
    [GetOrAddComponent, SerializeField, HideInInspector] private Animation animator;
    #endregion

    [SerializeField] private string fieldNameInSO;

    [Inject] private ControlSchemeChanger _controlSchemeChanger;
    #endregion

    #region ---EventFunctions---
    private void Start() {
        _controlSchemeChanger.OnControlSchemeChanged += OnControlSchemeChanged;
    }

    private void OnDestroy() {
        _controlSchemeChanger.OnControlSchemeChanged -= OnControlSchemeChanged;
    }
    #endregion

    private void OnControlSchemeChanged(IControlSchemeChanger.ControlSchemeData data) {
        animator.clip = data.DataSO.GetFieldByNameAndType<AnimationClip>(fieldNameInSO);
        animator.Play();
    }
}