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

    private void Awake()
    {
        _headStates = GetComponents<BossState>();
        _rightHandStates = rightHand.GetComponents<AUBHandState>();
        _leftHandStates = leftHand.GetComponents<AUBHandState>();
        
        // Remove the hover hand components from the arrays and store them
        _rightHandHover = rightHand.GetComponent<HoverHand>();
        _leftHandHover = leftHand.GetComponent<HoverHand>();
        _rightHandStates = Array.FindAll(_rightHandStates, state => state is not HoverHand);
        _leftHandStates = Array.FindAll(_leftHandStates, state => state is not HoverHand);

        foreach (var state in _headStates)
        {
            state.OnStateEnd += HeadLogic;
        }
        
        _boss = GetComponent<BossBase>();
        _boss.OnBattleStart += StartBattle;
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
        
        if (_leftHandHover.enabled && _rightHandHover.enabled)
        {
            // Roll a dice to see if both hands will attack or only one
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                var rightNextState = _rightHandStates[UnityEngine.Random.Range(0, _rightHandStates.Length)];
                rightHand.StateMachine.ForceSetState(rightNextState);
                
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
                leftHand.StateMachine.ForceSetState(leftNextState);
            }
            else
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
        }
    }
    
    private void HeadLogic()
    {
        if (!_isFighting) return;
        
        // Pick a random state from the list of states
        var nextState = _headStates[UnityEngine.Random.Range(0, _headStates.Length)];
        // Force the head to play the next state
        leftHand.StateMachine.ForceSetState(nextState);
    }
}
