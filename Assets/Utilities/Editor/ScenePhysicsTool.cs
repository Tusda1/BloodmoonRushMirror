#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Utilities.Editor {
    public class ScenePhysicsTool : EditorWindow {
        private const float TimeStepDuration = 1;

        [MenuItem("Tools/Scene Physics")]
        private static void OpenWindow() {
            GetWindow<ScenePhysicsTool>(false, "Physics", true);
        }
        
        private void OnGUI() {
            if (GUILayout.Button("Run Physics")) {
                _ = StepPhysics();
            }
        }

        private static async Task StepPhysics() {
            var gravityObjects = FindObjectsByType<GravityObject>(FindObjectsSortMode.None)
                .Where(x => x is not (CrusherScript or MovingPlatformScript or KeyScript));
        
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            const int gravityModifier = 1;
            var gravityForce = Physics2D.gravity * (gravityModifier * Time.fixedDeltaTime);
            var parameters = new object[] { gravityForce, true };
        
            var velocityField = typeof(GravityObject).GetField("velocity", bindingFlags);
            var movementMethod = typeof(GravityObject).GetMethod("Movement", bindingFlags);
        
            float time = 0;
            while (time < TimeStepDuration) {
                foreach (var gravityObject in gravityObjects) {
                    velocityField?.SetValue(gravityObject, gravityForce);
                    movementMethod?.Invoke(gravityObject, parameters);
                }

                await Task.Delay(200);
                time += 0.2f;

                Debug.Log(time);
            }
        }
    }
}
#endif