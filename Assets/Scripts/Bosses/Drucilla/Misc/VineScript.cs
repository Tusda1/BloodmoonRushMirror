using System;
using System.Collections;
using UnityEngine;

public class VineScript : MonoBehaviour 
{
    [SerializeField] private float amplitude = 1f; // Amplitude of the sine wave
    [SerializeField] private float frequency = 1f; // Frequency of the sine wave
    [SerializeField] private float speed = 1f;
    [SerializeField] private float resetSpeed = 1f;
    [SerializeField] private float maxHeight = 3f;
    
    private Collider2D _collider;
    private float _colliderLength;
    private Vector2 _startPosition;
    private Vector2 _currentPosition;
    private Vector2 _sinOffset;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _colliderLength = _collider.bounds.size.y;
        _startPosition = transform.position;
    }

    public void ActivateVine()
    {
        StartCoroutine(DeactivateVineRoutine());
    }
    
    private IEnumerator ActivateVineRoutine()
    {
        while (_currentPosition.y - _startPosition.y < maxHeight)
        {
            _currentPosition.y += speed * Time.deltaTime;
            _sinOffset.y = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = _currentPosition + _sinOffset;
            yield return new WaitForFixedUpdate();
        }

        while (true)
        {
            _sinOffset.y = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = _currentPosition + _sinOffset;
            yield return new WaitForFixedUpdate();
        }
    }
    
    public void DeactivateVine()
    {
        StopCoroutine(ActivateVineRoutine());
        StartCoroutine(DeactivateVineRoutine());
    }
    
    private IEnumerator DeactivateVineRoutine()
    {
        while (_currentPosition.y - _startPosition.y > 0)
        {
            _currentPosition.y = resetSpeed * Time.deltaTime;
            transform.position = _currentPosition;
            yield return new WaitForFixedUpdate();
        }
    }
}
