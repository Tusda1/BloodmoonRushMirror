#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CustomEditor(typeof(DialogueCharacter))]
public class DialogueCharacterCustomInspector : Editor {
    [FormerlySerializedAs("m_InspectorXML")] public VisualTreeAsset inspectorXML;

    public override VisualElement CreateInspectorGUI() {
        var myInspector = new VisualElement();
        inspectorXML.CloneTree(myInspector);
        myInspector.dataSource = target;

        var defaultInspector = new VisualElement();
        InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);
        myInspector.Add(defaultInspector);

        return myInspector;
    }
}
#endif