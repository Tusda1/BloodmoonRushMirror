using ExtensionMethods;
using Player;
using Unity.Cinemachine;
using UnityEngine;

public class MainCamera : MonoBehaviour {
    private PlayerScript _playerScript;

    [ContextMenu(nameof(SetAllCameraPositions))]
    public void SetAllCameraPositions() {
        _playerScript ??= FindAnyObjectByType<PlayerScript>();
        
        var cinemachineCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        var newPos = _playerScript.transform.position.WithOffset(y: 2);

        foreach (var cinemachineCamera in cinemachineCameras) {
            cinemachineCamera.ForceCameraPosition(newPos, Quaternion.identity);
        }
    }
}
