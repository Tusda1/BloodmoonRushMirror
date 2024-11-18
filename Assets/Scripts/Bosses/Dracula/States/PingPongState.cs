using UnityEngine;
using Player;

public class PingPongState : BossState 
{
    private GameObject _pingPongCircle;
    private GameObject _player;
    private int _pingPongHitCount;
    private Vector2 _directionToPingPong;
    private readonly float _pingPongStartSpeed = 5f;
    
    public override void Enter()
    {
        //Initialize variables
        _pingPongCircle = Resources.Load<GameObject>("Prefabs/PingPongCircle");
        _player = ServiceLocator.Get<PlayerScript>().gameObject;
        
        // Spawn ping pong circle and send it to the player
        var position = animator.transform.position;
        GameObject pingPongCircle = Object.Instantiate(_pingPongCircle, position, Quaternion.identity);
        
        // send the circle in the x direction of the player with no y movement
        _directionToPingPong = (_player.transform.position - position).normalized.x > 0 ? Vector2.right : Vector2.left;
        pingPongCircle.GetComponent<Rigidbody2D>().linearVelocity = _directionToPingPong * _pingPongStartSpeed;
    }
    
    public override void Update()
    {
        // Use Raycast to check if the pingpongcircle is close enough to be hit back
        RaycastHit2D hit = Physics2D.Raycast(animator.transform.position, _directionToPingPong, 1);
        if (hit.collider != null && hit.collider.CompareTag("PingPongCircle"))
        {
            if (hit.collider.attachedRigidbody.linearVelocity.normalized == _directionToPingPong)
            {
                return;
            }
            
            // Hit the circle back
            DamageInstance damageInstance = new DamageInstance();
            damageInstance.knockback = _directionToPingPong;
            
            hit.collider.GetComponent<IDamageable>().TakeDamage(damageInstance);
        }
    }
    
    public override void Exit()
    {
        Debug.Log("PingPongState Exit");
    }
}
