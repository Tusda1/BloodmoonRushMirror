using SceneCollections;
using UnityEngine;
using VContainer;

public class ExitBoilerLadder : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO nextLevelSceneCollection;
    
    private CameraEffects _cameraEffects;
    private IAchievementManager _achievementManager;
    private ISpeedrunTimer _speedrunTimer;
    #endregion
    
    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
        _achievementManager = container.Resolve<IAchievementManager>();
        _speedrunTimer = container.Resolve<ISpeedrunTimer>();
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            _cameraEffects.FadeLoadScene(nextLevelSceneCollection);
            _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_TheBoiler);
            _speedrunTimer.StopTimer();
        }
    }
}