using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;
#endif

public static class SceneCollectionManager {
    private static readonly List<SceneCollection.SceneInstance> ActiveScenes = new();
    private static readonly List<SceneCollection.SceneInstance> PersistentScenes = new();

    public static void LoadSceneCollection(SceneCollection sceneCollection) {
        foreach (var sceneInstance in ActiveScenes.ToArray()) {
            if (ActiveScenes.Contains(sceneInstance)) {
                if (!sceneInstance.getsReloadedWhenLoadedAgain) continue;
            }

            SceneManager.UnloadSceneAsync(sceneInstance.BuildIndex);
            ActiveScenes.Remove(sceneInstance);
        }

        foreach (var sceneInstance in sceneCollection.scenes) {
            if (!ActiveScenes.Contains(sceneInstance)) {
                SceneManager.LoadScene(sceneInstance.BuildIndex, LoadSceneMode.Additive);

                if (sceneInstance.persistentScene) {
                    PersistentScenes.Add(sceneInstance);
                }
                else {
                    ActiveScenes.Add(sceneInstance);
                }
            }

            if (sceneInstance.activeScene) {
                SceneManager.SetActiveScene(sceneInstance.LoadedScene);
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