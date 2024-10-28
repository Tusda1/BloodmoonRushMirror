using System;
using Attributes;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines;

namespace Bat {
    public class BatSpawner : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, HideInInspector] public SplineContainer batPath;

        [SerializeField] private float timeBetweenBats;
        [SerializeField] private int maxBats;

        private readonly RuntimeSet<Bat> _batRuntimeSet = new();

        private static ObjectPool<Bat> _batPool;
        private Transform _batParentTransform;

        private RepeatingTimer _timer;
        #endregion
        
        private void Start() {
            if (BatPoolManager.Instance == null) {
                throw new Exception($"{nameof(BatPoolManager)} does not exist!");
            }

            _batPool ??= BatPoolManager.Instance.Pool;

            _timer = new RepeatingTimer(timeBetweenBats);
            _timer.OnBegin += SpawnBat;
            _timer.StartTimer();
        }
        
#if UNITY_EDITOR
        private void OnValidate() {
            if (_timer != null) {
                _timer.Duration = timeBetweenBats;
            }
        }
#endif

        private void SpawnBat() {
            if (_batRuntimeSet.Count >= maxBats) return;

            var bat = _batPool.Get();
            _batRuntimeSet.Add(bat);

            bat.batSpawner = this;
        }

        public void DespawnBat(Bat bat) {
            _batPool.Release(bat);
            _batRuntimeSet.Remove(bat);
        }
    }
}