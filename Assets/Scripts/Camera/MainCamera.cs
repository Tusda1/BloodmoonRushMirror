using ExtensionMethods;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class MainCamera : MonoBehaviour {
    [Inject] private PlayerScript _playerScript;
    
    private void Awake() {
        var cinemachineCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        var newPos = _playerScript.transform.position.WithOffset(y: 2);

        foreach (var cinemachineCamera in cinemachineCameras) {
            cinemachineCamera.transform.position = newPos;
        }
    }
}
