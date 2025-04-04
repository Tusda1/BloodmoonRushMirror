using System;
using System.Collections.Generic;
using UnityEngine;

public class AUBBrain : MonoBehaviour {
    [SerializeField] private BossBase leftHand;
    [SerializeField] private BossBase rightHand;
    [SerializeField] private float handUpdateInterval = 5f;

    private float _handUpdateTimer;
    private bool _handsDestroyed;
    
    private AUBState[] _headStates;
    private AUBState[] _rightHandStates;
    private AUBState[] _leftHandStates;
    private HoverHand _rightHandHover;
    private HoverHand _leftHandHover;

    private void Awake()
    {
        _headStates = GetComponents<AUBState>();
        _rightHandStates = rightHand.GetComponents<AUBState>();
        _leftHandStates = leftHand.GetComponents<AUBState>();
        
        // Remove the hover hand components from the arrays and store them
        _rightHandHover = rightHand.GetComponent<HoverHand>();
        _leftHandHover = leftHand.GetComponent<HoverHand>();
        _rightHandStates = Array.FindAll(_rightHandStates, state => state is not HoverHand);
        _leftHandStates = Array.FindAll(_leftHandStates, state => state is not HoverHand);
    }

    private void Update()
    {
        if (!_handsDestroyed)
        {
            HandLogic();
        }
        else
        {
            HeadLogic();
        }
    }

    private void HandLogic()
    {
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
        
    }
}
