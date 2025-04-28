using System;
using Animancer;
using UnityEngine;

public class ShockwaveScript : MonoBehaviour 
{
    [SerializeField] private AnimationClip shockwaveAnimation;
    private AnimancerComponent _animancerComponent;
    private GameObject _shockwaveSpriteObject;
    private GameObject _shockwaveExtensionPrefab;
    private GameObject _rightShockWave;
    private GameObject _leftShockWave;
    private GameObject _rightAnimancer;
    private GameObject _leftAnimancer;
    
    private void Awake()
    {
        _animancerComponent = GetComponentInChildren<AnimancerComponent>();
        _shockwaveSpriteObject = transform.GetChild(0).gameObject;
        _shockwaveExtensionPrefab = Resources.Load<GameObject>("Prefabs/DrucillaShockwaveExtension");
        
        var state = _animancerComponent.Play(shockwaveAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            _shockwaveSpriteObject.SetActive(false);
            var position = transform.position;
            _rightShockWave = Instantiate(_shockwaveExtensionPrefab , position, Quaternion.identity);
            _leftShockWave = Instantiate(_shockwaveExtensionPrefab, position, Quaternion.identity);
            Vector3 localScale = _leftShockWave.transform.localScale;
            localScale.x = -1;
            _leftShockWave.transform.localScale = localScale;
        };
    }
}
