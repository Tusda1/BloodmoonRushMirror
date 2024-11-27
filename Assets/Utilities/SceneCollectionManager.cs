using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_EDITOR
using Directory = UnityEngine.Windows.Directory;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class SceneCollectionManager {
    #region ---Fields---
    private static SceneCollectionManagerSO SceneCollectionManagerSO {
        get {
            if (_sceneCollectionManagerSO != null) return _sceneCollectionManagerSO;
            
            _sceneCollectionManagerSO = ScrubUtils
                .GetAllScrubsInResourceFolder<SceneCollectionManagerSO>("SceneCollections")
                .GetByName("SceneCollectionManagerSO");

#if UNITY_EDITOR
            if (_sceneCollectionManagerSO == null) {
                var so = ScriptableObject.CreateInstance<SceneCollectionManagerSO>();
                const string folder = "Assets/Resources/SceneCollections";
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                    
                AssetDatabase.CreateAsset(so, Path.Join(folder,"SceneCollectionManagerSO.asset"));
                    
                _sceneCollectionManagerSO = so;
            }
#endif

            Debug.Log("Scene Collection Manager SO loaded manually");

            return _sceneCollectionManagerSO;
        }
    }

    private static SceneCollectionManagerSO _sceneCollectionManagerSO;
    
    private static List<SceneCollectionSO.SceneInstance> ActiveScenes => SceneCollectionManagerSO.activeScenes;
    private static List<SceneCollectionSO.SceneInstance> PersistentScenes => SceneCollectionManagerSO.persistentScenes;
    #endregion

    public static void AddToActiveScenes(SceneCollectionSO.SceneInstance scene) => ActiveScenes.Add(scene);

    public static async Task LoadSceneCollectionAsync(SceneCollectionSO sceneCollectionSO) {
#if UNITY_EDITOR
        if (ActiveScenes.Count == 0) {
            Debug.LogWarning("*** No ActiveScenes! ***");
            Debug.Log(CurrentSceneCollectionSO != null);

            foreach (var sceneInstance in CurrentSceneCollectionSO.scenes) {
                AddSceneInstanceToLists(sceneInstance);
            }
        }
#endif

        await SceneManager.LoadSceneAsync("EmptyScene", LoadSceneMode.Additive);

        foreach (var sceneInstance in sceneCollectionSO.notAllowedPersistentScenes) {
            foreach (var instance in PersistentScenes.Where(x => x.LoadedScene == sceneInstance.LoadedScene)) {
                PersistentScenes.Remove(instance);
                await SceneManager.UnloadSceneAsync(instance.LoadedScene);
            }
        }

        foreach (var sceneInstance in ActiveScenes.ToArray()) {
            if (sceneInstance.persistentScene) continue;
            if (sceneCollectionSO.scenes.Contains(sceneInstance) &&
                !sceneInstance.getsReloadedWhenLoadedAgain) continue;

            await SceneManager.UnloadSceneAsync(sceneInstance.BuildIndex, UnloadSceneOptions.None);
            Debug.Log($"[{nameof(SceneCollectionManager)}] Unloading: {sceneInstance.Name}");
            ActiveScenes.Remove(sceneInstance);
        }

        foreach (var sceneInstance in sceneCollectionSO.scenes) {
            if (!ActiveScenes.Contains(sceneInstance)) {
                await SceneManager.LoadSceneAsync(sceneInstance.BuildIndex, LoadSceneMode.Additive);
                Debug.Log($"[{nameof(SceneCollectionManager)}] Loading: {sceneInstance.Name}");

                AddSceneInstanceToLists(sceneInstance);
            }

            if (sceneInstance.activeScene) {
                SceneManager.SetActiveScene(sceneInstance.LoadedScene);
                Debug.Log($"[{nameof(SceneCollectionManager)}] Set Active Scene: {sceneInstance.Name}");
            }
        }

        SceneManager.UnloadSceneAsync("EmptyScene");
    }

    private static void AddSceneInstanceToLists(SceneCollectionSO.SceneInstance sceneInstance) {
        if (sceneInstance.persistentScene) {
            if (PersistentScenes.All(x => x.sceneReference.LoadedScene != sceneInstance.LoadedScene)) {
                PersistentScenes.Add(sceneInstance);
            }
        }

        ActiveScenes.Add(sceneInstance);
    }

#if UNITY_EDITOR
    public static SceneCollectionSO CurrentSceneCollectionSO;

    public static void LoadSceneCollectionEditor(SceneCollectionSO sceneCollectionSO) {
        CurrentSceneCollectionSO = sceneCollectionSO;

        ActiveScenes.Clear();

        EditorLoadScene(sceneCollectionSO.scenes.First(), false);

        for (var i = 1; i < sceneCollectionSO.scenes.Length; i++) {
            var sceneInstance = sceneCollectionSO.scenes[i];

            EditorLoadScene(sceneInstance);
        }
        
        foreach (var sceneInstance in sceneCollectionSO.notAllowedPersistentScenes) {
            foreach (var instance in PersistentScenes.Where(x => x.LoadedScene == sceneInstance.LoadedScene)) {
                PersistentScenes.Remove(instance);
                EditorSceneManager.CloseScene(instance.LoadedScene, true);
            }
        }

        foreach (var sceneInstance in PersistentScenes.Where(sceneInstance => !ActiveScenes.Contains(sceneInstance))) {
            EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path, OpenSceneMode.Additive);
            Debug.Log("Adding persistent scene not active");
        }
    }

    private static void EditorLoadScene(SceneCollectionSO.SceneInstance sceneInstance, bool additive = true) {
        AddSceneInstanceToLists(sceneInstance);

        if (additive) {
            EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path, OpenSceneMode.Additive);
        }
        else {
            EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path);
        }

        if (sceneInstance.activeScene) {
            SceneManager.SetActiveScene(sceneInstance.LoadedScene);
        }

        Debug.Log("Adding scene");
    }

    // private static IEnumerable<Scene> GetAllActiveScenes() {
    //     var sceneCount = SceneManager.sceneCount;
    //
    //     for (var i = 0; i < sceneCount; i++) {
    //         yield return SceneManager.GetSceneAt(i);
    //     }
    // }
#endif
}