using Attributes;
using DG.Tweening;
using Player;
using UnityEngine;

public class DrucillaFirstEncounter : MonoBehaviour, IDamageable {
    #region ---Fields---
    [SerializeField, Required] private ActionEventChannel drucillaInteractionChannel;
    [SerializeField] private DamageInstance drucillaDamageInstance = new() { knockback = Vector2.left };
    #endregion

    private void Start() {
        transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 0.5f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void TakeDamage(DamageInstance damageInstance) {
        DOTween.Kill(transform);
        
        ServiceLocator.Get<PlayerScript>().TakeDamage(drucillaDamageInstance);
        
        drucillaInteractionChannel.SendEventMessage();
    }
}