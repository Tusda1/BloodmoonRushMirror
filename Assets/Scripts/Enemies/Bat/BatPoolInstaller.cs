using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bat {
    public class BatPoolInstaller : LifetimeScope {
        [SerializeField] private GameObject batPrefab;
        [SerializeField] private int initialBatPoolSize = 15;
        [SerializeField] private Transform batParentTransform;

        protected override void Configure(IContainerBuilder builder) {
            CreateChild(new BatPoolManagerInstaller(batPrefab, initialBatPoolSize, batParentTransform));
        }
    }
}