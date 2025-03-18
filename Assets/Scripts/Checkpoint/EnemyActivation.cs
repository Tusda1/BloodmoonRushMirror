using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Checkpoint {
    public class EnemyActivation : MonoBehaviour {
        private GameObject[] _rooms;
        private readonly List<EnemyBase> _enemies = new();
        private VirtualCameraSwitch _checkpointRoom;

        public void Initialize(GameObject[] roomArray) {
            _rooms = roomArray;

            ServiceLocator.Get<PlayerScript>().OnRespawn += OnRespawn;

            foreach (var room in _rooms) {
                var enemyObjects = room.GetComponentsInChildren<EnemyBase>();

                _enemies.AddRange(enemyObjects);
            }
        }

        private void OnDestroy() {
            ServiceLocator.Get<PlayerScript>().OnRespawn -= OnRespawn;
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