using System.Collections.Generic;
using Player;
using UnityEngine;

public class EnemyActivation : MonoBehaviour {
    [HideInInspector] public GameObject[] rooms;
    [HideInInspector] public string enemiesLocation;
    private readonly List<GameObject> _enemies = new List<GameObject>();
    private VirtualCameraSwitch _checkpointRoom;

    public void Initialize() {
        ServiceLocator.Get<PlayerScript>().OnRespawn += OnRespawn;

        var enemyLayer = LayerMask.NameToLayer("Enemy");

        foreach (var room in rooms) {
            // Find the child GameObject named "AreaGameObject"
            var areaGameObject = room.transform.Find(enemiesLocation);

            // If AreaGameObject is not found, skip to the next room
            if (areaGameObject == null) continue;

            // Iterate through all children of AreaGameObject
            foreach (Transform child in areaGameObject) {
                // Check if child is on enemy layer
                if (child.gameObject.layer == enemyLayer) {
                    _enemies.Add(child.gameObject);
                    //child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnRespawn() {
        // Trigger the respawn of the enemies
        foreach (var enemy in _enemies) {
            // enemy.GetComponent<EnemyBase>().Respawn();
        }

        foreach (var room in rooms) {
            room.GetComponent<VirtualCameraSwitch>().EnemyDeactivation();
        }
    }
}