using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Shield
{
    [SerializeField] private bool alwaysCounterAttackOnDash = true;
    [SerializeField] private int blowsUntilBreak = 2;
    [SerializeField] private float breakResetTime = 5f;
    private float _lastHitTime;
    private int _breakCounter;
    private GravityObjectPlayerMovement _playerMovement;
    public event Action AttackBlocked;
    public event Action AttackNotBlocked;
    private Action _counterAttack;
    private MonoBehaviour _shieldUserMonoBehaviour;

    public void InitializeShield(Action counterAttack, MonoBehaviour shieldUserMonoBehaviour)
    {
        _counterAttack = counterAttack;
        _shieldUserMonoBehaviour = shieldUserMonoBehaviour;
    }
    
    public bool ShieldBlock(PlayerScript player, DamageInstance damageInstance, Action shieldBreakAction = null)
    {
        if (!_playerMovement) _playerMovement = player.GetComponent<GravityObjectPlayerMovement>();
        
        if (damageInstance.which != DamageInstance.WhichAttack.CrouchAttack)
        {
            AttackBlocked?.Invoke();
            
            if (damageInstance.which == DamageInstance.WhichAttack.DashAttack)
            {
                _shieldUserMonoBehaviour.StartCoroutine(DashCancelCoroutine());
            }

            return true;
        }
        
        if (Time.time - _lastHitTime > breakResetTime)
        {
            _breakCounter = 0;
        }
        
        AttackNotBlocked?.Invoke();
        _breakCounter++;
        _lastHitTime = Time.time;
        if (_breakCounter >= blowsUntilBreak)
        {
            shieldBreakAction?.Invoke();
            _breakCounter = 0;
        }
        
        return false;
    }
    
    private IEnumerator DashCancelCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        if (_playerMovement.DashTimer.IsRunning) _playerMovement.DashTimer.StopTimer();
        if (alwaysCounterAttackOnDash)
        {
            _counterAttack?.Invoke();
        }
    }
}
