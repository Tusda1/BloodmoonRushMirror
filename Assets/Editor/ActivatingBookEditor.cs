using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivatingBook))]
public class ActivatingBookEditor : Editor {
    #region ---Fields---
    #endregion

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ActivatingBook book = (ActivatingBook)target;
        if (GUILayout.Button("Add Book")) book.AddBook();
        if (GUILayout.Button("Remove Book")) book.RemoveBook();
        
        EditorUtility.SetDirty(target);
    }
    
}
