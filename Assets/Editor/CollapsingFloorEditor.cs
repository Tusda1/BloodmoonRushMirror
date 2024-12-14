using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollapsingFloor))]
public class CollapsingFloorEditor : Editor {
    #region ---Fields---
    #endregion

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        CollapsingFloor floor = (CollapsingFloor)target;
        
        if (GUILayout.Button("Set Exploding Tiles")) floor.SetExplodingTiles();
        
        if (GUILayout.Button("Explode")) floor.Explode();
        
        EditorUtility.SetDirty(target);
    }
}
