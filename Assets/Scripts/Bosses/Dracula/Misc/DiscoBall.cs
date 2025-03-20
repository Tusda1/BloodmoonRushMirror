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
    private bool _isActive;
    
    private GameObject _laserPrefab;
    private Pooler _pooler;
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

    public void Enable()
    {
        if (_isActive) return;
        
        _currentRotation = narrowFireAngle;
        _isActive = true;
        
        StartCoroutine(LowerDiscoBall());
    }

    private void Disable()
    {
        _isActive = false;
        StopAllCoroutines();
        _currentRotation = 0;
    }

    private IEnumerator LowerDiscoBall()
    {
        while (transform.position.y > _lowPointPosition.y)
        {
            transform.position += Vector3.down * (loweringSpeed * Time.deltaTime);
            yield return null;
        }
        
        StartCoroutine(RotateDiscoBall());
    }
    
    private IEnumerator RaiseDiscoBall()
    {
        while (transform.position.y < _highPointPosition.y)
        {
            transform.position += Vector3.up * (loweringSpeed * Time.deltaTime);
            yield return null;
        }
        
        Disable();
    }
    
    private IEnumerator RotateDiscoBall()
    {
        while (_currentRotation < 180 - narrowFireAngle)
        {
            Fire();
            _currentRotation += rotationInterval;
            yield return new WaitForSeconds(fireInterval);
        }
        
        
    }

    private void Fire()
    {
        // Use rotation to determine the direction of the laser
        var direction = Quaternion.Euler(0, 0, _currentRotation) * Vector2.left;
        var laser = _pooler.GetPooledObject();
        laser.transform.position = transform.position;
        laser.GetComponent<ProjectileDeflectable>().velocityDirection = direction;
        laser.SetActive(true);
    }
}
