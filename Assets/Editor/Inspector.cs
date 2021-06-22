using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Wave))]
public class Inspector : Editor
{
    
    public override void OnInspectorGUI()
    {
        Wave wave = (Wave)target;
        GUILayout.Label("1 = Robot\n2 = Assassin\n3 = Walker\n4 = Protector\n5 = Rollermine\n6 = Commander");
        base.OnInspectorGUI();
        /*
        GUILayout.BeginHorizontal();
        GUILayout.ExpandWidth(true); ;
        SerializedProperty tps = serializedObject.FindProperty("enemyCounts");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        SerializedProperty max = serializedObject.FindProperty("maxOf");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(max, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        EditorGUIUtility.LookLikeControls();
        GUILayout.EndHorizontal();*/
    }
}