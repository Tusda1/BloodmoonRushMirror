using UnityEngine;
using UnityEngine.Pool;

namespace Bat {
    public class BatPoolManager : Singleton<BatPoolManager> {
        public GameObject batPrefab;
        public int batPoolSize;
        [SerializeField] private Transform batParentTransform;

        public ObjectPool<Bat> Pool;

        protected override void Awake() {
            base.Awake();

            CreateBatPool();
        }

        private void CreateBatPool() {
            batParentTransform ??= new GameObject("BatPoolParent").transform;

            Pool = new ObjectPool<Bat>(CreateBat, ActionOnGet, ActionOnRelease, ActionOnDestroy, true, batPoolSize);

            var bats = new Bat[batPoolSize];
            for (var i = 0; i < batPoolSize; i++) {
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