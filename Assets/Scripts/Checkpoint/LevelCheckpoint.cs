using Animancer;
using Player;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Triggers;
using UnityEngine;
using Utilities.DataBinding;
using Action = System.Action;
using System.Collections;
using Attributes;
using ExtensionMethods;
using UnityEngine.Rendering.Universal;
using VContainer;

public partial class LevelCheckpoint : TriggerBox, IBind<CheckpointData> {
    #region ---Fields---
    #region ***---GetOrAddComponents---****
    [GetOrAddComponent, SerializeField, HideInInspector] private AnimancerComponent animancerComponent;
    #endregion

    [SerializeField] private string currentProgressTitle;
    
    [Header("Animations")]
    [SerializeField] private AnimationClip inactiveIdle;
    [SerializeField] private AnimationClip activeIdle;
    [SerializeField] private AnimationClip activeStart;

    private Light2D _lightComponent;
    private float _lightRange;
    
    public event Action CheckpointReached = delegate { };
    
    private ISaveLoadSystemManager _saveLoadSystemManager;
    private LevelDataManager _levelDataManager;
    private IAudioManager _audioManager;
    private PlayerScript _playerScript;
    
    #region ***---SoundID---***
    private const string CheckpointReached_SoundID = "Checkpoint_Reached";
    private const string CheckpointOnIdle_SoundID = "Checkpoint_OnIdle";
    #endregion
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
        _levelDataManager = container.Resolve<LevelDataManager>();
        _audioManager = container.Resolve<IAudioManager>();
        _playerScript = container.Resolve<PlayerScript>();
    }

    private void Awake() {
        animancerComponent ??= GetComponent<AnimancerComponent>();
        _lightComponent ??= GetComponentInChildren<Light2D>();
    }

    private void Start() {
        Bind();
        
        animancerComponent.Play(hasBeenTriggered ? activeIdle : inactiveIdle);
        _lightRange = _lightComponent.pointLightOuterRadius;
        _lightComponent.pointLightOuterRadius = 0;
    }
    #endregion

    protected override void TriggeredAction(Collider2D other) {
        _audioManager.PlayOneShotAttached(CheckpointReached_SoundID, gameObject);

        CheckpointReached.Invoke();
        _playerScript.GetComponent<GravityObjectPlayerMovement>().lastPlatformCheckpoint = transform.position;

        var state = animancerComponent.Play(activeStart);
        state.Events(this).OnEnd = () => animancerComponent.Play(activeIdle);
        StartCoroutine(TurnOnLights());

        _playerScript.CheckpointHealing();
        
        _levelDataManager.DataInGameData.CurrentCheckpointID = ID;
        _levelDataManager.DataInGameData.PlayerPosition = transform.position.WithOffset(y: 0.015024f);
        _levelDataManager.DataInGameData.currentProgress = currentProgressTitle != string.Empty 
            ? currentProgressTitle 
            : name;
        
        _saveLoadSystemManager.SaveAllDataBinds();
    }

    private IEnumerator TurnOnLights()
    {
        float length = activeStart.length;
        float time = 0;
        while (time < length)
        {
            time += Time.deltaTime;
            _lightComponent.pointLightOuterRadius = Mathf.Lerp(0, _lightRange, time / length);
            yield return null;
        }
        
        _lightComponent.pointLightOuterRadius = _lightRange;
    }
    
    public void TryStartOnIdle() {
        if (hasBeenTriggered) {
            _audioManager.PlayOneShotAttached(CheckpointOnIdle_SoundID, gameObject);
        }
    }

    public void StopOnIdle() {
        _audioManager.StopAllSFX();
    }
}