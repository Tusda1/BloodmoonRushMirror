using Attributes;
using DG.Tweening;
using Player;
using UnityEngine;

public class DrucillaFirstEncounter : MonoBehaviour, IDamageable {
    #region ---Fields---
    [SerializeField, Required] private ActionEventChannel drucillaInteractionChannel;
    [SerializeField] private DamageInstance drucillaDamageInstance = new() { knockback = Vector2.left };
    
    private Vector3 _startPosition;
    #endregion

    private void Start() {
        _startPosition = transform.position;
        
        transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 0.5f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void TakeDamage(DamageInstance damageInstance) {
        DOTween.Kill(transform);
        transform.position = _startPosition;
        
        ServiceLocator.Get<PlayerScript>().TakeDamage(drucillaDamageInstance);
        
        var cameraEffects = ServiceLocator.Get<CameraEffects>();
        _ = cameraEffects.ShakeCamera(0.6f, 0.4f);
        
        drucillaInteractionChannel.SendEventMessage();
    }
}