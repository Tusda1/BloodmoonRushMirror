using Attributes;
using Unity.VisualScripting;
using UnityEngine;

public class FallingVoid : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField] private ScriptMachine cutsceneGraph;
    #endregion

    private void Start() {
        cutsceneGraph.TriggerUnityEvent("Start");
    }
}