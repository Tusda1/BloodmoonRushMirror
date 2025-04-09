using System;
using System.Collections.Generic;
using UnityEngine;

public class AUBBrain : MonoBehaviour {
    [SerializeField] private BossBase leftHand;
    [SerializeField] private BossBase rightHand;
    
    private bool _handsDestroyed;
    private bool _isFighting;
    
    private BossBase _boss;
    private BossState[] _headStates;
    private AUBHandState[] _rightHandStates;
    private AUBHandState[] _leftHandStates;
    private HoverHand _rightHandHover;
    private HoverHand _leftHandHover;
    private HandKilledState _rightHandKilledState;
    private HandKilledState _leftHandKilledState;
    

    private void Awake()
    {
        _headStates = GetComponents<BossState>();
        _rightHandStates = rightHand.GetComponents<AUBHandState>();
        _leftHandStates = leftHand.GetComponents<AUBHandState>();
        
        foreach (var state in _headStates)
        {
            state.OnStateEnd += HeadLogic;
        }
        
        // Remove the hover hand components from the arrays and store them
        _rightHandHover = rightHand.GetComponent<HoverHand>();
        _leftHandHover = leftHand.GetComponent<HoverHand>();
        _rightHandKilledState = rightHand.GetComponent<HandKilledState>();
        _leftHandKilledState = leftHand.GetComponent<HandKilledState>();
        _rightHandStates = Array.FindAll(_rightHandStates, state => state is not HoverHand);
        _leftHandStates = Array.FindAll(_leftHandStates, state => state is not HoverHand);
        _headStates = Array.FindAll(_headStates, state => state is not IdleHeadState);
        _headStates = Array.FindAll(_headStates, state => state is not HeadHitByHandState);
        
        _boss = GetComponent<BossBase>();
        _boss.OnBattleStart += StartBattle;
        leftHand.OnDeath += StartHandKillSequence;
        rightHand.OnDeath += StartHandKillSequence;
        _leftHandKilledState.OnHandKilled += HandHitScreen;
        _rightHandKilledState.OnHandKilled += HandHitScreen;
    }
    
    private void StartBattle()
    {
        _isFighting = true;
    }

    private void Update()
    {
        if (!_handsDestroyed)
        {
            HandLogic();
        }
    }

    private void HandLogic()
    {
        if (!_isFighting) return;
        
        if (_leftHandHover.isHovering && _rightHandHover.isHovering)
        {
            // Roll a dice to see if both hands will attack or only one
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                DoubleHandLogic();
                return;
            }
            
            SingleHandLogic();
            return;
        }
        
        if (!leftHand.gameObject.activeSelf && _rightHandHover.isHovering)
        {
            var rightNextState = _rightHandStates[UnityEngine.Random.Range(0, _rightHandStates.Length)];
            rightHand.StateMachine.ForceSetState(rightNextState);
        }
        else if (!rightHand.gameObject.activeSelf && _leftHandHover.isHovering)
        {
            var leftNextState = _leftHandStates[UnityEngine.Random.Range(0, _leftHandStates.Length)];
            leftHand.StateMachine.ForceSetState(leftNextState);
        }
    }

    private void DoubleHandLogic()
    {
        var rightNextState = _rightHandStates[UnityEngine.Random.Range(0, _rightHandStates.Length)];
                
        var leftNextState = _leftHandStates[UnityEngine.Random.Range(0, _leftHandStates.Length)];
        var acceptedAttack = false;
        while (!acceptedAttack)
        {
            if (leftNextState != rightNextState)
            {
                acceptedAttack = true;
            }
            else
            {
                if (leftNextState.canBeMirrored)
                {
                    acceptedAttack = true;
                }
                else
                {
                    leftNextState = _leftHandStates[UnityEngine.Random.Range(0, _leftHandStates.Length)];
                }
            }
        }
        rightHand.StateMachine.ForceSetState(rightNextState);
        leftHand.StateMachine.ForceSetState(leftNextState);
    }

    private void SingleHandLogic()
    {
        // Roll a dice to see which hand will attack
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            var nextState = _rightHandStates[UnityEngine.Random.Range(0, _rightHandStates.Length)];
            rightHand.StateMachine.ForceSetState(nextState);
        }
        else 
        {
            var nextState = _leftHandStates[UnityEngine.Random.Range(0, _leftHandStates.Length)];
            leftHand.StateMachine.ForceSetState(nextState);
        }
    }
    
    private void HeadLogic()
    {
        if (!leftHand.gameObject.activeSelf && !rightHand.gameObject.activeSelf)
        {
            _handsDestroyed = true;
        }
        
        if (!_handsDestroyed || !_isFighting)
        {
            _boss.StateMachine.ForceSetDefaultState();
            return;
        }
      
        // Pick a random state from the list of states
        var nextState = _headStates[UnityEngine.Random.Range(0, _headStates.Length)];
        // Force the head to play the next state
        _boss.StateMachine.ForceSetState(nextState);
    }

    private void StartHandKillSequence()
    {
        if (leftHand.gameObject.activeSelf && leftHand.CurrentHealth <= 0)
        {
            if (leftHand.StateMachine.CurrentState is not HandKilledState)
            {
                var killedState = leftHand.GetComponent<HandKilledState>();
                leftHand.StateMachine.ForceSetState(killedState);
            }
        }
        else
        {
            leftHand.StateMachine.ForceSetState(_leftHandHover);
        }
        
        if (rightHand.gameObject.activeSelf && rightHand.CurrentHealth <= 0)
        {
            if (rightHand.StateMachine.CurrentState is not HandKilledState)
            {
                var killedState = rightHand.GetComponent<HandKilledState>();
                rightHand.StateMachine.ForceSetState(killedState);
            }
        }
        else
        {
            rightHand.StateMachine.ForceSetState(_rightHandHover);
        }
    }

    private void HandHitScreen()
    {
        _boss.StateMachine.ForceSetState(GetComponent<HeadHitByHandState>());
    }
}
