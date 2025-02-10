#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities.Editor {
    public class ObjectReplaceTool : EditorWindow {
        private GameObject _prefab;

        [MenuItem("Tools/Replace Tool")]
        public static void ShowWindow() {
            GetWindow<ObjectReplaceTool>("Replace Tool");
        }

        private void OnGUI() {
            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);

            if (Selection.gameObjects.Length == 0) {
                GUILayout.Label("Select some GameObjects in the hierarchy to replace.");
                return;
            }

            if (_prefab != null) {
                if (GUILayout.Button($"Replace Selection With Prefab {_prefab.name}")) {
                    Replace();
                }
            }
            else {
                GUI.enabled = false;
                GUILayout.Button("Replace Selection With Prefab {none}");
                GUI.enabled = true;

                var oldColor = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label("Select a Prefab above first to replace any selection.");
                GUI.color = oldColor;
            }
        }

        private void Replace() {
            var selectedObjects = Selection.gameObjects;

            foreach (var selectedObject in selectedObjects) {
                var newObject = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;
                Assert.IsNotNull(newObject);
                newObject.transform.position = selectedObject.transform.position;
                newObject.transform.rotation = selectedObject.transform.rotation;
                newObject.transform.parent = selectedObject.transform.parent;

                if (selectedObject.TryGetComponent<Ladder>(out var ladder) && newObject.TryGetComponent(out Ladder newLadder)) {
                    var height = typeof(Ladder).GetField("height", BindingFlags.NonPublic | BindingFlags.Instance);
                    height?.SetValue(newLadder, height.GetValue(ladder));
                    newLadder.UpdateLadderHeight();
                }

                DestroyImmediate(selectedObject);
            }
        }
    }
}
#endif