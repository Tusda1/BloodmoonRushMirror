using Animancer;
using Player;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Triggers;
using UnityEngine;
using Utilities.DataBinding;
using Action = System.Action;
using System;
using Attributes;
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

    public CheckpointData DataInGameData => _levelDataManager.DataInGameData.checkpointDatas.GetOrAddByIdInList(ID,
        new CheckpointData {
            name = name,
            ID = ID,
            HasBeenTriggered = hasBeenTriggered,
            Position = transform.position
        });
    #endregion

    public event Action CheckpointReached = delegate { };
    
    private SaveLoadSystemManager _saveLoadSystemManager;
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
        _saveLoadSystemManager = container.Resolve<SaveLoadSystemManager>();
        _levelDataManager = container.Resolve<LevelDataManager>();
        _audioManager = container.Resolve<IAudioManager>();
        _playerScript = container.Resolve<PlayerScript>();
    }

    private void Awake() {
        animancerComponent ??= GetComponent<AnimancerComponent>();
    }

    private void Start() {
        Bind();
        
        animancerComponent.Play(hasBeenTriggered ? activeIdle : inactiveIdle);
    }
    #endregion

    protected override void TriggeredAction(Collider2D other) {
        _audioManager.PlayOneShot(CheckpointReached_SoundID);
        _audioManager.PlayOneShotAttached(CheckpointOnIdle_SoundID, gameObject);

        CheckpointReached.Invoke();

        var state = animancerComponent.Play(activeStart);
        state.Events(this).OnEnd = () => animancerComponent.Play(activeIdle);

        _playerScript.CheckpointHealing();

        SaveBindData();
    }

    public void StopOnIdle() {
        _audioManager.StopAllSFX();
    }

    #region ---DataBindingMethods---
    public void Bind() {
        _saveLoadSystemManager.Bind(this, DataInGameData);
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