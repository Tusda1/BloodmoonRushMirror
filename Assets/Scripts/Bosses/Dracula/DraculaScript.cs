using System;
using System.Collections.Generic;
using UnityEngine;


public class DraculaScript : GravityObject
{
    private Animator _animator;
    private BossStateMachine _stateMachine;
    private Dictionary<string, Dictionary<string, Type>> _states = new Dictionary<string, Dictionary<string, Type>>();
    private enum StateGroups
    {
        General,
        Phase1,
        Phase2,
    }

    public float velocityReferenceX;
    public float velocityReferenceY;
    
    private new void Start()
    {
        base.Start();
        _stateMachine = new BossStateMachine();
        _animator = GetComponent<Animator>();
        
        AddState(StateGroups.General.ToString(), "Teleport", typeof(TeleportState));
        AddState(StateGroups.Phase1.ToString(), "PingPong", typeof(PingPongState));
        
        TransitionToState(_states[StateGroups.Phase1.ToString()]["PingPong"]);
    }

    private new void Update()
    {
        base.Update();
        _stateMachine.Update();
        
        targetVelocity = new Vector2(velocityReferenceX, 0);
        velocity = new Vector2(0, velocityReferenceY);
    }
    
    private void AddState(string groupName, string stateName, Type type)
    {
        if (!_states.ContainsKey(groupName))
        {
            _states.Add(groupName, new Dictionary<string, Type>());
        }
        
        _states[groupName].Add(stateName, type);
    }
    
    private void TransitionToState(Type stateType)
    {
        var state = (BossState) Activator.CreateInstance(stateType);
        state.Initialize(_animator);
        _stateMachine.SetState(state);
    }
}
