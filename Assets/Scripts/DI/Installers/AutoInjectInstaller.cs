using VContainer.Unity;
#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VContainer;
#endif

public class AutoInjectInstaller : LifetimeScope {
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

    private void OnValidate() {
        RegisterAutoInjectObjects();
        EditorUtility.SetDirty(this);
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

        if (type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(property => property.GetCustomAttribute<InjectAttribute>() != null)) {
            return true;
        }

        return type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(method => method.GetCustomAttribute<InjectAttribute>() != null);
    }

    protected void SetReferenceFromSceneIfNull<T>(ref T @object) where T : Component {
        @object ??= FindObjectsByType<T>(FindObjectsSortMode.None)
            .Single(x => x.gameObject.scene == gameObject.scene);
    }
#endif
}