using System.Collections;
using UnityEngine;

public class DraculaScript : GravityObject
{
    private GameObject _pingPongCircle;
    private Coroutine _currentAttack;
    
    private void Start()
    {
        _pingPongCircle = Resources.Load<GameObject>("Prefabs/PingPongCircle");
        
        _currentAttack = StartCoroutine(PingPongAttack());
    }

    private IEnumerator PingPongAttack()
    {
        // Spawn ping pong circle and send it to the player
        GameObject pingPongCircle = Instantiate(_pingPongCircle, transform.position, Quaternion.identity);
        
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
