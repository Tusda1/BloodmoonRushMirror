using Attributes;
using PlayerLoopTimer;
using TetraCreations.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines;
using VContainer;

namespace Bat {
    public class BatSpawner : MonoBehaviour {
        #region ---Fields---
        #region ---GetOrAddComponents---
        [GetOrAddComponent, HideInInspector] public SplineContainer batPath;
        #endregion

        #region ---Inspector---
        [Header("Bat Settings")]
        [SerializeField] private float timeBetweenBats = 2;
        [SerializeField] private float batMoveSpeed = 5;
        [SerializeField] private bool limitBatAmount;
        [SerializeField, DrawIf(nameof(limitBatAmount), true)] private int maxBats = 20;
        #endregion

        #region ---PrivateFields---
        private readonly RuntimeSet<Bat> _batRuntimeSet = new();
        private ObjectPool<Bat> _batPool;
        private RepeatingTimer _timer;
        #endregion
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            _batPool = container.Resolve<BatPoolManager>().Pool;
        }

        private void Start() {
            _timer = new RepeatingTimer(timeBetweenBats);
            _timer.OnBegin += SpawnBat;
            _timer.StartTimer();
        }
        #endregion
        
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
#if UNITY_EDITOR
        private void OnValidate() {
            if (_timer != null) {
                _timer.Duration = timeBetweenBats;
            }
        }
#endif
    }
}