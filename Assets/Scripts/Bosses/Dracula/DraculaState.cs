using Animancer;
using UnityEngine;

public class DraculaState : BossState 
{
    [SerializeField] private DraculaStateCategory category;
    public DraculaStateCategory Category => category;
    
    public bool StateFinished { get; protected set; }
}
