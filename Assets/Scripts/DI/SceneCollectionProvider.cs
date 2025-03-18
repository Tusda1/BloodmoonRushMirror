using System.Collections.Generic;
using SceneCollections;
using VContainer;

public class SceneCollectionProvider : IProvider<SceneCollectionSO> {
    private readonly Dictionary<string, SceneCollectionSO> _services = new();

    [Inject]
    public SceneCollectionProvider(SceneCollectionSO[] sceneCollections) {
        foreach (var sceneCollection in sceneCollections) {
            _services.Add(sceneCollection.name, sceneCollection);
        }
    }

    Dictionary<string, SceneCollectionSO> IProvider<SceneCollectionSO>.Services => _services;
}