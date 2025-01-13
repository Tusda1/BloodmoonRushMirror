using System;
using UnityEngine;

public class HomingProjectile : MonoBehaviour 
{
    [HideInInspector] public GameObject target;
    [HideInInspector] public Vector2 initialDirection;
    public float launchDistance = 3f;
    
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float homingSpeed = 5f;
    
    private Rigidbody2D _rb2d;

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _rb2d.linearVelocity = initialDirection.normalized * launchSpeed;
    }
}