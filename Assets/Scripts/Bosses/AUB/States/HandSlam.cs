using System;
using System.Collections;
using Player;
using UnityEngine;

public class HandSlam : AUBHandState
{
    [SerializeField] private AnimationClip slamAnimation;
    [SerializeField] private float heightAbovePlayer = 4f;
    [SerializeField] private float waitAfterSlam = 0.5f;
    [SerializeField] private float slamSpeed = 10f;
    
    private PlayerScript _playerScript;

    private void Awake()
    {
        _playerScript = ServiceLocator.Get<PlayerScript>();
    }

    private void OnEnable()
    {
        StartCoroutine(MoveToSlamPosition());
    }

    private IEnumerator MoveToSlamPosition()
    {
        var destination = new Vector2(-1000, -1000);
        while (Vector2.Distance(Boss.bossCollider.bounds.center, destination) > 0.5f)
        {
            destination = new Vector2(_playerScript.transform.position.x, _playerScript.transform.position.y + heightAbovePlayer);
            Boss.Move(destination - (Vector2)Boss.bossCollider.bounds.center);
            yield return null;
        }
        
        StartCoroutine(Slam());
    }
    
    private IEnumerator Slam()
    {
        Boss.AnimancerComponent.Play(slamAnimation);
        while (!Boss.bossCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            Boss.Move(Vector2.down * slamSpeed);
            yield return null;
        }
        
        yield return new WaitForSeconds(waitAfterSlam);
        
        Boss.StateMachine.ForceSetState(hoverHand);
    }
}
