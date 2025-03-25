using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bat {
    public class BatPoolManagerInstaller : IInstaller {
        private readonly GameObject _batPrefab;
        private readonly int _initialBatPoolSize;
        private readonly Transform _batParentTransform;

        public BatPoolManagerInstaller(GameObject batPrefab, int initialBatPoolSize, Transform batParentTransform) {
            _batPrefab = batPrefab;
            _initialBatPoolSize = initialBatPoolSize;
            _batParentTransform = batParentTransform;
        }

        public void Install(IContainerBuilder builder) {
            builder.Register<BatPoolManager>(Lifetime.Singleton)
                .WithParameter("batPrefab", _batPrefab)
                .WithParameter("initialBatPoolSize", _initialBatPoolSize)
                .WithParameter("batParentTransform", _batParentTransform);
        }
    }
}