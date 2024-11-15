using UnityEngine;
using Player;
using Unity.VisualScripting;

public class DraculaPingPong : StateMachineBehaviour
{
    private GameObject _pingPongCircle;
    private GameObject _player;
    private int _pingPongHitCount;
    private Vector2 _directionToPingPong;
    [SerializeField] private float _pingPongStartSpeed;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Initialize variables
        _pingPongCircle = Resources.Load<GameObject>("Prefabs/PingPongCircle");
        _player = ServiceLocator.Get<PlayerScript>().gameObject;
        
        // Spawn ping pong circle and send it to the player
        var position = animator.transform.position;
        GameObject pingPongCircle = Instantiate(_pingPongCircle, position, Quaternion.identity);
        
        // send the circle in the x direction of the player with no y movement
        _directionToPingPong = (_player.transform.position - position).normalized.x > 0 ? Vector2.right : Vector2.left;
        pingPongCircle.GetComponent<Rigidbody2D>().linearVelocity = _directionToPingPong * _pingPongStartSpeed;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
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

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
