using System.Collections;
using Player;
using Unity.Cinemachine;
using UnityEngine;

public class VirtualCameraSwitch : MonoBehaviour {
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private GameObject areaGameObjects;
    [SerializeField] private GameObject EnemyGameObject;
    private bool _isDeactivating;
    [HideInInspector] public bool isActiveRoom;

    private static int _priority;

    private void Awake() {
        //Check if player is in this room
        isActiveRoom = cinemachineCamera.Priority > 0;
    }
    
    private void Start() {
        areaGameObjects.SetActive(false);
        EnemyGameObject.SetActive(false);
        
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
        }
    }
    
    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DelayedDeactivation());
        }
    }
    
    private IEnumerator DelayedDeactivation() {
        _isDeactivating = true;
        yield return new WaitForSeconds(0.2f);
        if (!_isDeactivating)
        {
            yield break;
        }
        ManualDeactivation();
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
        EnemyGameObject.SetActive(true);
    }
    
    public void EnemyDeactivation() {
        EnemyGameObject.SetActive(false);
    }
}