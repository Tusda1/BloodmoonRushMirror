using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Reflection",
        story: "Call [functionName] In [script]",
        category: "DialogueAction",
        id: "2aa36e2dee0f965713092da12b0ad796")]
    public class ReflectionAction : Action {
        [SerializeReference] public BlackboardVariable<string> functionName;
        [SerializeReference] public BlackboardVariable<MonoBehaviour> script;

        protected override Status OnStart() {
            if (script == null || string.IsNullOrEmpty(functionName.Value)) {
                return Status.Failure;
            }

            var methodInfo = script.Value.GetType().GetMethod(functionName.Value);

            if (methodInfo == null) {
                Debug.LogError($"Method {functionName.Value} not found on {script.Value.GetType().Name}");
                return Status.Failure;
            }

            methodInfo.Invoke(script.Value, null);

            return Status.Success;
        }
    }
}