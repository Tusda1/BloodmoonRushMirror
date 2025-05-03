using System;
using System.Collections;
using Animancer;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using VContainer;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class DiscoBall : MonoBehaviour 
{
    [SerializeField] private float rotationInterval = 15f;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float loweringSpeed = 2f;
    [SerializeField] private float narrowFireAngle = 15f;
    [SerializeField] private GameObject lowPoint;
    [SerializeField] private GameObject highPoint;
    [SerializeField] private AnimationClip discoBallSpinAnimation;
    
    private Vector2 _lowPointPosition;
    private Vector2 _highPointPosition;
    private float _rotationTimer;
    private float _currentRotation;
    private bool _isFiring;
    private bool _isActive;
    private EventInstance _discoBallSoundInstance;
    
    private GameObject _laserPrefab;
    private Pooler _pooler;
    private Coroutine _moveCoroutine;
    private IObjectResolver _objectResolver;
    private IAudioManager _audioManager;
    private AnimancerComponent _animancerComponent;

    [Inject]
    public void Construct(IObjectResolver container) {
        _objectResolver = container;
        _audioManager = container.Resolve<IAudioManager>();
    }

    private void Awake()
    {
        _lowPointPosition = lowPoint.transform.position;
        _highPointPosition = highPoint.transform.position;
        transform.position = _highPointPosition;
        _laserPrefab = Resources.Load<GameObject>("Prefabs/Laser");
        _animancerComponent = GetComponent<AnimancerComponent>();
    }

    private void Start() {
        _pooler = TryGetCommunityPooler.TryGetPooler(_laserPrefab, _objectResolver);
        _discoBallSoundInstance = RuntimeManager.CreateInstance("event:/Sfx/Boss/Dracula/Drac_disco_lower");
    }

    public void ResetDiscoBall()
    {
        StopAllCoroutines();
        transform.position = _highPointPosition;
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
        _animancerComponent.Play(discoBallSpinAnimation);
        StartCoroutine(RotateDiscoBall());
    }
    
    private void DisableLaser()
    {
        _currentRotation = 0;
        _animancerComponent.Stop(discoBallSpinAnimation);
        _isFiring = false;
    }

    private IEnumerator LowerDiscoBall()
    {
        _discoBallSoundInstance.start();
        
        while (transform.position.y > _lowPointPosition.y)
        {
            transform.position += Vector3.down * (loweringSpeed * Time.deltaTime);
            yield return null;
        }
        
        _discoBallSoundInstance.stop(STOP_MODE.ALLOWFADEOUT);
        _isActive = true;
        _moveCoroutine = null;
        EnableLaser();
    }
    
    private IEnumerator RaiseDiscoBall()
    {
        _discoBallSoundInstance.start();
        
        while (transform.position.y < _highPointPosition.y)
        {
            transform.position += Vector3.up * (loweringSpeed * Time.deltaTime);
            yield return null;
        }
        
        _discoBallSoundInstance.stop(STOP_MODE.ALLOWFADEOUT);
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
        _audioManager.PlayOneShotAttached("Drac_Disco_Shoot", gameObject);
    }
}
