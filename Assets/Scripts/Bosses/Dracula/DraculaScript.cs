using System.Collections;
using UnityEngine;
using Player;

public class DraculaScript : GravityObject
{
    private BossStateMachine _stateMachine;
    
    private Animator _animator;
    private GameObject _pingPongCircle;
    private Coroutine _currentAttack;
    private GameObject _player;
    
    private void Start()
    {
        _stateMachine = new BossStateMachine();
        _animator = GetComponent<Animator>();
        _pingPongCircle = Resources.Load<GameObject>("Prefabs/PingPongCircle");
        _player = ServiceLocator.Get<PlayerScript>().gameObject;
        
        _currentAttack = StartCoroutine(PingPongAttack());
    }

    private IEnumerator PingPongAttack()
    {
        // Spawn ping pong circle and send it to the player
        GameObject pingPongCircle = Instantiate(_pingPongCircle, transform.position, Quaternion.identity);
        // send the circle in the x direction of the player with no y movement
        Vector2 direction = (_player.transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
        pingPongCircle.GetComponent<Rigidbody2D>().linearVelocity = direction * 5;
        
        yield return null;
    }
    
    private IEnumerator FireballAttack()
    {
        yield return null;
    }
    
    private IEnumerator BatAttack()
    {
        yield return null;
    }
}
