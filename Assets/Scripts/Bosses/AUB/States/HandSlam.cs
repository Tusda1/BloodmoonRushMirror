using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

[DefaultExecutionOrder(1)]
public class HandSlam : AUBHandState
{
    [SerializeField] private float heightAbovePlayer = 4f;
    [SerializeField] private float waitAfterSlam = 0.5f;
    [SerializeField] private float moveSpeed = 10f;
    
    private PlayerScript _playerScript;
    [SerializeField] private HandSlamLogic handSlamLogic = new();
    [Inject] private CameraEffects _cameraEffects;

    private void OnEnable()
    {
        handSlamLogic.Initialize(Boss, _cameraEffects);
        
        if (_playerScript == null) _playerScript = ServiceLocator.Get<PlayerScript>();
        
        StartCoroutine(MoveToSlamPosition());
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator MoveToSlamPosition()
    {
        var destination = new Vector2(-1000, -1000);
        while (Vector2.Distance(Boss.bossCollider.bounds.center, destination) > 0.5f)
        {
            destination = new Vector2(_playerScript.transform.position.x, _playerScript.transform.position.y + heightAbovePlayer);
            Boss.Move((destination - (Vector2)Boss.bossCollider.bounds.center).normalized * moveSpeed);
            yield return null;
        }
        
        StartCoroutine(Slam());
    }
    
    private IEnumerator Slam()
    {
        while (!handSlamLogic.ExecuteHandSlamLogic())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(waitAfterSlam);
        
        Boss.StateMachine.ForceSetState(hoverHand);
    }
}
