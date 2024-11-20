using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public static class SceneCollectionManager {
    private static readonly List<SceneCollection.SceneInstance> ActiveScenes = new();
    private static readonly List<SceneCollection.SceneInstance> PersistentScenes = new();

    public static async Task LoadSceneCollection(SceneCollection sceneCollection) {
        foreach (var sceneInstance in ActiveScenes.ToArray()) {
            if (sceneInstance.persistentScene) continue;
            if (sceneCollection.scenes.Contains(sceneInstance) && !sceneInstance.getsReloadedWhenLoadedAgain) continue;

            await SceneManager.UnloadSceneAsync(sceneInstance.BuildIndex);
            Debug.Log("unloading: " + sceneInstance.Name);
            ActiveScenes.Remove(sceneInstance);
        }

        foreach (var sceneInstance in sceneCollection.scenes) {
            if (!ActiveScenes.Contains(sceneInstance)) {
                await SceneManager.LoadSceneAsync(sceneInstance.BuildIndex, LoadSceneMode.Additive);
                Debug.Log("Loading: " + sceneInstance.Name);

                if (sceneInstance.persistentScene) {
                    PersistentScenes.Add(sceneInstance);
                }
                ActiveScenes.Add(sceneInstance);
                
            }

            if (sceneInstance.activeScene) {
                SceneManager.SetActiveScene(sceneInstance.LoadedScene);
                Debug.Log("Set Active Scene: " + sceneInstance.Name);
            }
        }
    }

#if UNITY_EDITOR
    public static void LoadSceneCollectionEditor(SceneCollection sceneCollection) {
        ActiveScenes.Clear();

        EditorLoadScene(sceneCollection.scenes.First(), false);

        for (var i = 1; i < sceneCollection.scenes.Length; i++) {
            var sceneInstance = sceneCollection.scenes[i];

            EditorLoadScene(sceneInstance);
        }

        foreach (var sceneInstance in PersistentScenes.Where(sceneInstance => !ActiveScenes.Contains(sceneInstance))) {
            EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path, OpenSceneMode.Additive);
            Debug.Log("Adding persistent scene not active");
        }
    }

    private static void EditorLoadScene(SceneCollection.SceneInstance sceneInstance, bool additive = true) {
        if (sceneInstance.persistentScene) {
            if (!PersistentScenes.Contains(sceneInstance)) {
                PersistentScenes.Add(sceneInstance);
            }
        }

        if (additive) {
            EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path, OpenSceneMode.Additive);
        }
        else {
            EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path);
        }

        if (sceneInstance.activeScene) {
            SceneManager.SetActiveScene(sceneInstance.LoadedScene);
        }

        ActiveScenes.Add(sceneInstance);
        Debug.Log("Adding scene");
    }

    private static IEnumerable<Scene> GetAllScenes() {
        var countLoaded = SceneManager.sceneCount;
    
        for (var i = 0; i < countLoaded; i++) {
            yield return SceneManager.GetSceneAt(i);
        }
    }
#endif
}