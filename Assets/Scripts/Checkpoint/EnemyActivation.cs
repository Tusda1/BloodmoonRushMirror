using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Checkpoint {
    public class EnemyActivation : MonoBehaviour {
        private GameObject[] _rooms;
        private readonly List<EnemyBase> _enemies = new();
        private VirtualCameraSwitch _checkpointRoom;
        private PlayerScript _player;

        public void Initialize(GameObject[] roomArray) {
            _rooms = roomArray;
            // _enemiesLocation = enemiesLocation;
            _player = ServiceLocator.Get<PlayerScript>();

            _player.OnRespawn += OnRespawn;

            foreach (var room in _rooms) {
                var enemyObjects = room.GetComponentsInChildren<EnemyBase>();

                _enemies.AddRange(enemyObjects);
            }
        }

        private void OnDestroy() {
            if (_player == null) return;
            
            _player.OnRespawn -= OnRespawn;
        }

        private void OnRespawn() {
            foreach (var enemy in _enemies) 
            {
                enemy.Respawn();
            }

            foreach (var room in _rooms) 
            {
                room.GetComponent<VirtualCameraSwitch>().EnemyDeactivation();
            }
        }
    }
}