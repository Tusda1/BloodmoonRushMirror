using UnityEngine;

public class BatAttackState : BossState
{
    [SerializeField] private float attackLength = 5f;
    
    private GameObject _batPrefab;
    private Collider2D _collider;
    private float _timeSinceAttackStart;
    
    private void OnEnable()
    {
        _batPrefab = Resources.Load<GameObject>("Prefabs/DraculaBat");
        _collider = GetComponent<Collider2D>();
        _timeSinceAttackStart = Time.time;
    }
    
    public void Update()
    {
        // Spawn a bat every 0.5 seconds randomly on the left side of the collider
        if (Time.time % 0.5f < 0.1f)
        {
            var position = _collider.bounds.min;
            position.y += Random.Range(0, _collider.bounds.size.y);
            Instantiate(_batPrefab, position, Quaternion.identity);
        }
        
        // If the attack has been going on for 5 seconds, switch to the next state
        if (Time.time - _timeSinceAttackStart > attackLength)
        {
            Boss.StateMachine.ForceSetDefaultState();
        }
    }
}
