using SaveLoadSystem;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;
#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
#endif

public class LevelInstaller : LifetimeScope {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private LevelDataManager levelDataManager;
    [SerializeField] private string leaderboardID;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(mainCamera);
        builder.RegisterInstance(cinemachineBrain);
        builder.RegisterInstance(levelDataManager);

        builder.RegisterInstance(new SteamSpeedrunTimer(leaderboardID)).As<ISpeedrunTimer>();
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(RegisterAutoInjectObjects))]
    public void RegisterAutoInjectObjects() {
        var gameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(x => x.scene == gameObject.scene);
        
        autoInjectGameObjects.Clear();

        foreach (var o in gameObjects) {
            if (HasMonoBehaviorWithInjectAttribute(o)) {
                autoInjectGameObjects.Add(o);
            }
        }
    }

    [ContextMenu(nameof(TrySetReferences))]
    public void TrySetReferences() {
        mainCamera ??= FindObjectsByType<Camera>(FindObjectsSortMode.None)
            .Single(x => x.scene == gameObject.scene);
        
        cinemachineBrain ??= FindObjectsByType<CinemachineBrain>(FindObjectsSortMode.None)
            .Single(x => x.transform.gameObject.scene == gameObject.scene);
        
        levelDataManager ??= FindObjectsByType<LevelDataManager>(FindObjectsSortMode.None)
            .Single(x => x.transform.gameObject.scene == gameObject.scene);
    }

    private static bool HasMonoBehaviorWithInjectAttribute(GameObject gameObject) {
        return gameObject.GetComponents<MonoBehaviour>().Any(HasInjectAttribute);
    }


    private static bool HasInjectAttribute(MonoBehaviour behaviour) {
        var type = behaviour.GetType();

        if (type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(field => field.GetCustomAttribute<InjectAttribute>() != null)) {
            return true;
        }

        if (type.GetProperties(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(property => property.GetCustomAttribute<InjectAttribute>() != null)) {
            return true;
        }

        return type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(method => method.GetCustomAttribute<InjectAttribute>() != null);
    }
#endif
}