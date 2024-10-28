using Attributes;
using UnityEngine;
using UnityEngine.Pool;

namespace Bat {
    public class BatPoolManager : Singleton<BatPoolManager> {
        #region ---Fields---
        public int initialBatPoolSize;
        [SerializeField] private Transform batParentTransform;
        [AssetPreviewIcon(80, 80)] public GameObject batPrefab;

        public ObjectPool<Bat> Pool;
        #endregion

        protected override void Awake() {
            base.Awake();

            CreateBatPool();
        }

        private void CreateBatPool() {
            batParentTransform ??= new GameObject("BatPoolParent").transform;

            Pool = new ObjectPool<Bat>(CreateBat, ActionOnGet, ActionOnRelease, ActionOnDestroy, true,
                initialBatPoolSize);

            var bats = new Bat[initialBatPoolSize];
            for (var i = 0; i < initialBatPoolSize; i++) {
                bats[i] = Pool.Get();
            }

            foreach (var bat in bats) {
                Pool.Release(bat);
            }
        }

        private void ActionOnDestroy(Bat bat) {
            Destroy(bat.gameObject);
        }

        private void ActionOnRelease(Bat bat) {
            bat.gameObject.SetActive(false);
        }

        private void ActionOnGet(Bat bat) {
            bat.gameObject.SetActive(true);
        }

        private Bat CreateBat() {
            var obj = Instantiate(batPrefab, batParentTransform);
            obj.SetActive(false);
            return obj.GetComponent<Bat>();
        }
    }
}