using System;
using System.Collections;
using UnityEngine;
using VContainer;

public class DiscoBall : MonoBehaviour 
{
    [SerializeField] private float rotationInterval = 15f;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float loweringSpeed = 2f;
    [SerializeField] private float narrowFireAngle = 15f;
    [SerializeField] private GameObject lowPoint;
    [SerializeField] private GameObject highPoint;
    
    private Vector2 _lowPointPosition;
    private Vector2 _highPointPosition;
    private float _rotationTimer;
    private float _currentRotation;
    private bool _isFiring;
    private bool _isActive;
    
    private GameObject _laserPrefab;
    private Pooler _pooler;
    private Coroutine _moveCoroutine;
    private IObjectResolver _objectResolver;

    [Inject]
    public void Construct(IObjectResolver container) {
        _objectResolver = container;
    }

    private void Awake()
    {
        _lowPointPosition = lowPoint.transform.position;
        _highPointPosition = highPoint.transform.position;
        transform.position = _highPointPosition;
        _laserPrefab = Resources.Load<GameObject>("Prefabs/Laser");
    }

    private void Start() {
        _pooler = TryGetCommunityPooler.TryGetPooler(_laserPrefab, _objectResolver);
    }

    public void Activate()
    {
        if (_moveCoroutine != null)
        {
            return;
        }
        
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(LowerDiscoBall());
    }

    public void Deactivate()
    {
        _isActive = false;
        StopAllCoroutines();
        _moveCoroutine = StartCoroutine(RaiseDiscoBall());
    }
    
    public void EnableLaser()
    {
        if (!_isActive)
        {
            Activate();
            return;
        }
        
        if (_isFiring)
        {
            return;
        }
        
        _isFiring = true;
        _currentRotation = narrowFireAngle;
        StartCoroutine(RotateDiscoBall());
    }
    
    private void DisableLaser()
    {
        _currentRotation = 0;
        _isFiring = false;
    }

    private IEnumerator LowerDiscoBall()
    {
        while (transform.position.y > _lowPointPosition.y)
        {
            transform.position += Vector3.down * (loweringSpeed * Time.deltaTime);
            yield return null;
        }
        
        _isActive = true;
        _moveCoroutine = null;
        EnableLaser();
    }
    
    private IEnumerator RaiseDiscoBall()
    {
        while (transform.position.y < _highPointPosition.y)
        {
            transform.position += Vector3.up * (loweringSpeed * Time.deltaTime);
            yield return null;
        }
        
        _moveCoroutine = null;
    }
    
    private IEnumerator RotateDiscoBall()
    {
        while (_currentRotation < 180 - narrowFireAngle)
        {
            Fire();
            _currentRotation += rotationInterval;
            yield return new WaitForSeconds(fireInterval);
        }
        
        DisableLaser();
    }

    private void Fire()
    {
        // Use rotation to determine the direction of the laser
        var direction = Quaternion.Euler(0, 0, _currentRotation) * Vector2.left;
        var laser = _pooler.GetPooledObject();
        laser.transform.position = transform.position;
        laser.GetComponent<ProjectileNotSolid>().targetVector = direction;
        laser.SetActive(true);
    }
}
