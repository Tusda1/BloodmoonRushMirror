using System.Collections;
using Attributes;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class VirtualCameraSwitch : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField, Required] private CinemachineCamera cinemachineCamera;
    [SerializeField, Required] private GameObject areaGameObjects;
    [SerializeField, Required] private GameObject enemyGameObject;

    [SerializeField] private UnityEvent onEnable;
    [SerializeField] private UnityEvent onEnableLate;
    [SerializeField] private UnityEvent onDisable;
    [SerializeField] private UnityEvent onDisableLate;
 
    [HideInInspector] public bool isActiveRoom;

    private bool _isDeactivating;
    private static int _priority;
    private Coroutine _coroutine;
    #endregion

    private void Awake() {
        isActiveRoom = cinemachineCamera.Priority > 0;
    }

    private void Start() {
        areaGameObjects.SetActive(false);
        enemyGameObject.SetActive(false);

        if (cinemachineBrain == null) {
            cinemachineBrain = Camera.main?.GetComponent<CinemachineBrain>();
        }

        if (cinemachineCamera.Follow == null) {
            cinemachineCamera.Follow = ServiceLocator.Get<PlayerScript>().transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            ManualActivation();
            _coroutine = StartCoroutine(DelayedActivation());
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            StartCoroutine(DelayedDeactivation());
            
            if (_coroutine != null) {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    private IEnumerator DelayedDeactivation() {
        onDisable.Invoke();
        _isDeactivating = true;
        yield return new WaitForSeconds(cinemachineBrain.DefaultBlend.Time);
        if (!_isDeactivating) {
            yield break;
        }

        ManualDeactivation();
        onDisableLate.Invoke();
    }
    
    private IEnumerator DelayedActivation() {
        onEnable.Invoke();
        yield return new WaitForSeconds(cinemachineBrain.DefaultBlend.Time);
        onEnableLate.Invoke();
    }

    public void ManualActivation() {
        cinemachineCamera.Priority = _priority++;
        areaGameObjects.SetActive(true);
        EnemyActivation();
        isActiveRoom = true;
        _isDeactivating = false;
    }

    public void ManualDeactivation() {
        cinemachineCamera.Priority -= 5;
        areaGameObjects.SetActive(false);
        isActiveRoom = false;
    }

    private void EnemyActivation() {
        enemyGameObject.SetActive(true);
    }

    public void EnemyDeactivation() {
        enemyGameObject.SetActive(false);
    }
}