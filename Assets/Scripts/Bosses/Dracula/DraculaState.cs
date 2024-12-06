using System;
using Animancer;
using UnityEngine;

public class DraculaState : BossState 
{
    [SerializeField] private DraculaStateCategory category;
    public DraculaStateCategory Category => category;
    
    protected Dracula DraculaReference => (Dracula)Boss;
    
    public event Action OnStateEnd;
    
    protected void EndState()
    {
        OnStateEnd?.Invoke();
    }
}
