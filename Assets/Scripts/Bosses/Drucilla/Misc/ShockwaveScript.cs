using System;
using Animancer;
using UnityEngine;

public class ShockwaveScript : MonoBehaviour 
{
    [SerializeField] private AnimationClip shockwaveAnimation;
    private AnimancerComponent _animancerComponent;
    private GameObject _shockwaveSpriteObject;
    private GameObject _shockwaveExtensionPrefab;
    
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
            Instantiate(_shockwaveExtensionPrefab, position, Quaternion.identity);
            Instantiate(_shockwaveExtensionPrefab, position, Quaternion.identity).transform.localScale = new Vector3(-1, 1, 1);
            Destroy(gameObject);
        };
    }
}
