using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Ladder))]
public class LadderEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        var ladder = (Ladder)target;
        
        if (GUILayout.Button(nameof(ladder.UpdateLadderHeight))) {
            ladder.UpdateLadderHeight();
        }
    }
}
