using System.Collections;
using Attributes;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class VirtualCameraSwitch : MonoBehaviour {
    #region ---Fields---
    [SerializeField, Required] private CinemachineCamera cinemachineCamera;
    [SerializeField, Required] private GameObject areaGameObjects;
    [SerializeField, Required] private GameObject enemyGameObject;
    
    [Header("TimeSlowTransition")]
    [SerializeField] private bool slowTimeOnTransition = true;
    [SerializeField] private float timeSlowDuration = 2;
    [SerializeField] private AnimationCurve timeSlowCurve;

    [SerializeField] private UnityEvent onEnable;
    [SerializeField] private UnityEvent onEnableLate;
    [SerializeField] private UnityEvent onDisable;
    [SerializeField] private UnityEvent onDisableLate;
 
    [HideInInspector] public bool isActiveRoom;

    private bool _isDeactivating;
    private static int _priority;
    private Coroutine _coroutine;
    
    [Inject] private PlayerScript _playerScript;
    [Inject] private CinemachineBrain _cinemachineBrain;
    [Inject] private CameraEffects _cameraEffects;
    #endregion

    private void Awake() {
        isActiveRoom = cinemachineCamera.Priority > 0;
    }

    private void Start() {
        areaGameObjects.SetActive(false);
        enemyGameObject.SetActive(false);

        if (cinemachineCamera.Follow == null) {
            cinemachineCamera.Follow = _playerScript.transform;
        }
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    #region ---Activation---
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            ManualActivation();
            _coroutine = StartCoroutine(DelayedActivation());
        }
    }

    private void ManualActivation() {
        cinemachineCamera.Priority = 1;
        
        if (slowTimeOnTransition) {
            _ = _cameraEffects.TimeSlow(timeSlowDuration, timeSlowCurve);
        }
        
        areaGameObjects.SetActive(true);
        EnemyActivation();
        
        isActiveRoom = true;
        _isDeactivating = false;
    }

    private void EnemyActivation() {
        enemyGameObject.SetActive(true);
    }
    
    private IEnumerator DelayedActivation() {
        onEnable.Invoke();
        yield return new WaitForSeconds(_cinemachineBrain.DefaultBlend.Time);
        onEnableLate.Invoke();
    }
    #endregion

    #region ---Deactivation---
    private void OnTriggerExit2D(Collider2D other) {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        
        if (other.CompareTag("Player")) {
            StartCoroutine(DelayedDeactivation());
            
            if (_coroutine != null) {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }

    private void ManualDeactivation() {
        cinemachineCamera.Priority = 0;
        areaGameObjects.SetActive(false);
        isActiveRoom = false;
    }

    public void EnemyDeactivation() {
        enemyGameObject.SetActive(false);
    }

    private IEnumerator DelayedDeactivation() {
        onDisable.Invoke();
        _isDeactivating = true;
        yield return new WaitForSeconds(_cinemachineBrain.DefaultBlend.Time);
        if (!_isDeactivating) {
            yield break;
        }

        ManualDeactivation();
        onDisableLate.Invoke();
    }
    #endregion
}