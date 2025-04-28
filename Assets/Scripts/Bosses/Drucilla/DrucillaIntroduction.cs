using Attributes;
using DG.Tweening;
using Player;
using SceneCollections;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class DrucillaIntroduction : MonoBehaviour, IDamageable {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private ScriptMachine cutsceneGraph;
    
    [SerializeField, Required] private SceneCollectionSO nextLevelSceneCollection;
    [SerializeField] private DamageInstance drucillaDamageInstance = new() { knockback = Vector2.left };
    
    private Vector3 _startPosition;
    
    private CameraEffects _cameraEffects;
    private PlayerScript _playerScript;
    private ISpeedrunTimer _speedrunTimer;
    private IAchievementManager _achievementManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
        _playerScript = container.Resolve<PlayerScript>();
        _speedrunTimer = container.Resolve<ISpeedrunTimer>();
        _achievementManager = container.Resolve<IAchievementManager>();
    }

    private void Start() {
        _startPosition = transform.position;
        
        transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 0.5f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void TakeDamage(DamageInstance damageInstance) {
        DOTween.Kill(transform);
        transform.position = _startPosition;
        
        _speedrunTimer.StopTimer();
        
        _playerScript.TakeDamage(drucillaDamageInstance);
        
        _ = _cameraEffects.ShakeCamera(0.6f, 0.4f);
        
        cutsceneGraph.TriggerUnityEvent("Start");
    }

    [UnityEngine.Scripting.Preserve]
    public void EndDialogue() {
        _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_EntranceHall);
        _cameraEffects.FadeLoadScene(nextLevelSceneCollection);
    }
}