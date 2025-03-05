using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Checkpoint {
    public class EnemyActivation : MonoBehaviour {
        private GameObject[] _rooms;
        private string _enemiesLocation;
        private readonly List<EnemyBase> _enemies = new();
        private VirtualCameraSwitch _checkpointRoom;

        public void Initialize(GameObject[] roomArray, string enemiesLocation) {
            _rooms = roomArray;
            _enemiesLocation = enemiesLocation;

            ServiceLocator.Get<PlayerScript>().OnRespawn += OnRespawn;

            foreach (var room in _rooms) {
                // Find the child GameObject named "AreaGameObject"
                var areaGameObject = room.transform.Find(_enemiesLocation);

                // If AreaGameObject is not found, skip to the next room
                if (areaGameObject == null) continue;

                // Iterate through all children of AreaGameObject
                foreach (Transform child in areaGameObject) {
                    // Check if child has enemybase component
                    if (child.TryGetComponent<EnemyBase>(out var enemy)) {
                        _enemies.Add(enemy);
                        print(enemy.transform.position);
                    }
                }
            }
            print($"Found {_enemies.Count} enemies in {_rooms.Length} rooms");
        }

        private void OnRespawn() {
            foreach (var enemy in _enemies) {
                print(enemy.gameObject.name);
                enemy.Respawn();
            }

            foreach (var room in _rooms) {
                room.GetComponent<VirtualCameraSwitch>().EnemyDeactivation();
            }
        }
    }
}