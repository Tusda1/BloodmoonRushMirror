using Animancer;
using Player;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Triggers;
using UnityEngine;
using Utilities.DataBinding;
using Action = System.Action;
using System;
using System.Collections;
using Attributes;
using UnityEngine.Rendering.Universal;
using VContainer;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelCheckpoint : TriggerBox, IBind<CheckpointData> {
    #region ---Fields---
    #region ***---GetOrAddComponents---****
    [GetOrAddComponent, SerializeField, HideInInspector] private AnimancerComponent animancerComponent;
    #endregion
    
    [Header("Animations")]
    [SerializeField] private AnimationClip inactiveIdle;
    [SerializeField] private AnimationClip activeIdle;
    [SerializeField] private AnimationClip activeStart;
    
    

    #region ***---DataBindingVariables---***
    [field: Header("Data Binding")]
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public CheckpointData SaveSystemData { get; set; }

    public CheckpointData DataInGameData => _levelDataManager.DataInGameData.checkpointDatas
        .GetOrAddByIDInList(ID, CheckpointDataGenerator);
    
    private Func<CheckpointData> CheckpointDataGenerator => () => new CheckpointData {
        name = name,
        ID = ID,
        HasBeenTriggered = hasBeenTriggered,
        Position = transform.position
    };
    #endregion

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
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#endif
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

        var state = animancerComponent.Play(activeStart);
        state.Events(this).OnEnd = () => animancerComponent.Play(activeIdle);
        StartCoroutine(TurnOnLights());

        _playerScript.CheckpointHealing();
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager.SaveAllDataBinds();
#endif
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

    #region ---DataBindingMethods---
    public void Bind() {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
#endif
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
            ID = Guid.NewGuid().ToString();
            Debug.Log($"Setting new guid for {nameof(LevelDataManager)}");
        }
    }
#endif

    [ContextMenu(nameof(LoadBindData))]
    public void LoadBindData() {
        name = DataInGameData.name;
        hasBeenTriggered = DataInGameData.HasBeenTriggered;
        transform.position = DataInGameData.Position;
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        DataInGameData.name = name;
        DataInGameData.HasBeenTriggered = hasBeenTriggered;
        DataInGameData.Position = transform.position;
    }
    #endregion
}