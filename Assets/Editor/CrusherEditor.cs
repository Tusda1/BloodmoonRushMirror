using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrusherManager))]
public class CrusherEditor : Editor {
    #region ---Fields---
    #endregion
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        CrusherManager crusher = (CrusherManager)target;
        if (GUILayout.Button("Generate Crusher Tiles")) crusher.GenerateCrusherTiles();
        
        EditorUtility.SetDirty(target);
    }
}
