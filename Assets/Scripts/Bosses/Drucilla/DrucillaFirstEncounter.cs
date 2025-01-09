using Attributes;
using DG.Tweening;
using Player;
using SceneCollections;
using UnityEngine;

public class DrucillaFirstEncounter : MonoBehaviour, IDamageable {
    #region ---Fields---
    [SerializeField, Required] private ActionEventChannel drucillaInteractionChannel;
    [SerializeField, Required] private SceneCollectionSO endDemoSceneCollection;
    [SerializeField] private DamageInstance drucillaDamageInstance = new() { knockback = Vector2.left };
    
    private Vector3 _startPosition;
    private CameraEffects _cameraEffects;
    #endregion

    private void Start() {
        _startPosition = transform.position;
        
        transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 0.5f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        _cameraEffects = ServiceLocator.Get<CameraEffects>();
    }

    public void TakeDamage(DamageInstance damageInstance) {
        DOTween.Kill(transform);
        transform.position = _startPosition;
        
        ServiceLocator.Get<PlayerScript>().TakeDamage(drucillaDamageInstance);
        
        _ = _cameraEffects.ShakeCamera(0.6f, 0.4f);
        
        drucillaInteractionChannel.SendEventMessage();
    }

    public void EndDemo() {
        _cameraEffects.CircleFadeOut(1, 
            () => _ = SceneCollectionManager.LoadSceneCollectionAsync(endDemoSceneCollection));
    }
}