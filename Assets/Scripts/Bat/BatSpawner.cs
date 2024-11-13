using System;
using Attributes;
using PlayerLoopTimer;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines;

namespace Bat {
    public class BatSpawner : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, HideInInspector] public SplineContainer batPath;

        [Header("Bat Settings")]
        [SerializeField] private float timeBetweenBats = 2;
        [SerializeField] private float batMoveSpeed = 5;
        [SerializeField] private bool limitBatAmount;
        [SerializeField, DrawIf(nameof(limitBatAmount), true)] private int maxBats = 20;

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
            if (limitBatAmount && _batRuntimeSet.Count >= maxBats) return;

            var bat = _batPool.Get();
            _batRuntimeSet.Add(bat);

            bat.batSpawner = this;
            bat.splineAnimate.MaxSpeed = batMoveSpeed;
        }

        public void DespawnBat(Bat bat) {
            _batPool.Release(bat);
            _batRuntimeSet.Remove(bat);
        }
    }
}