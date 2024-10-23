using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Call Function",
        story: "Call [functionName] in [script]",
        category: "DialogueAction",
        id: "2aa36e2dee0f965713092da12b0ad796")]
    public class CallFunctionAction : Action {
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