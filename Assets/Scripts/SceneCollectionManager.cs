using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public static class SceneCollectionManager {
    private static readonly List<SceneCollection.SceneInstance> ActiveScenes = new();

    public static void LoadSceneCollection(SceneCollection sceneCollection) {
        foreach (var sceneInstance in ActiveScenes.ToArray()) {
            if (sceneInstance.persistentScene) continue;

            if (ActiveScenes.Contains(sceneInstance)) {
                if (!sceneInstance.getsReloadedWhenLoadedAgain) continue;
            }

            SceneManager.UnloadSceneAsync(sceneInstance.BuildIndex);
            ActiveScenes.Remove(sceneInstance);
        }

        foreach (var sceneInstance in sceneCollection.scenes) {
            if (!ActiveScenes.Contains(sceneInstance)) {
                SceneManager.LoadScene(sceneInstance.BuildIndex, LoadSceneMode.Additive);
                ActiveScenes.Add(sceneInstance);
            }

            if (sceneInstance.activeScene) {
                SceneManager.SetActiveScene(sceneInstance.LoadedScene);
            }
        }
    }

#if UNITY_EDITOR
    public static void LoadSceneCollectionEditor(SceneCollection sceneCollection) {
        EditorLoadScene(sceneCollection.scenes.First(), false);
        
        for (var index = 1; index < sceneCollection.scenes.Length; index++) {
            var sceneInstance = sceneCollection.scenes[index];
            EditorLoadScene(sceneInstance);
        }
    }

    private static void EditorLoadScene(SceneCollection.SceneInstance sceneInstance, bool additive = true) {
        ActiveScenes.Clear();
        
        if (!ActiveScenes.Contains(sceneInstance)) {
            if (additive) {
                EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path, OpenSceneMode.Additive);
            }
            else {
                EditorSceneManager.OpenScene(sceneInstance.sceneReference.Path);
            }
            ActiveScenes.Add(sceneInstance);
        }

        if (sceneInstance.activeScene) {
            SceneManager.SetActiveScene(sceneInstance.LoadedScene);
        }
    }

    private static IEnumerable<Scene> GetAllScenes() {
        var countLoaded = SceneManager.sceneCount;

        for (var i = 0; i < countLoaded; i++) {
            yield return SceneManager.GetSceneAt(i);
        }
    }
#endif
}