using System.Collections;
using UnityEngine;
using Player;

public class DraculaScript : GravityObject
{
    private Animator _animator;
    

    public float velocityReferenceX;
    public float velocityReferenceY;
    
    private new void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
    }

    private new void Update()
    {
        base.Update();

        targetVelocity = new Vector2(velocityReferenceX, 0);
        velocity = new Vector2(0, velocityReferenceY);
    }

    private IEnumerator PingPongAttack()
    {
        
        
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
