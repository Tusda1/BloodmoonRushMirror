using Attributes;
using UnityEngine;
using VContainer;
using ExtensionMethods;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationFromControlScheme : MonoBehaviour {
    #region ---Fields---
    #region ***---GetOrAddComponents---***
    [GetOrAddComponent, SerializeField, HideInInspector] private SpriteRenderer spriteRenderer;
    [GetOrAddComponent, SerializeField, HideInInspector] private Animator animator;
    #endregion

    [SerializeField] private string fieldNameInSO;

    [Inject] private IControlSchemeChanger _controlSchemeChanger;
    
    private PlayableGraph _graph;
    private AnimationPlayableOutput _output;
    private AnimationClipPlayable _clipPlayable;
    #endregion

    #region ---EventFunctions---
    private void Start() {
        _graph = PlayableGraph.Create();
        _output = AnimationPlayableOutput.Create(_graph, "Animation", animator);
        
        _controlSchemeChanger.OnControlSchemeChanged += OnControlSchemeChanged;
        OnControlSchemeChanged(_controlSchemeChanger.CurrentControlSchemeData);
    }

    private void OnDestroy() {
        _controlSchemeChanger.OnControlSchemeChanged -= OnControlSchemeChanged;
        
        if (_graph.IsValid()) _graph.Destroy();
    }
    #endregion

    private void OnControlSchemeChanged(IControlSchemeChanger.ControlSchemeData data) {
        var animationClip = data.DataSO.GetFieldByNameAndType<AnimationClip>(fieldNameInSO);
        
        if (animationClip == null) return;

        if (_clipPlayable.IsValid()) {
            _clipPlayable.Destroy();
        }

        _clipPlayable = AnimationClipPlayable.Create(_graph, animationClip);
        _output.SetSourcePlayable(_clipPlayable);
        _graph.Play();
    }
}