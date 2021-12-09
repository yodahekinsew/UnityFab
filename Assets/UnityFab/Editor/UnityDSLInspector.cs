using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(UnityDSL))]
public class UnityDSLInspector : Editor
{
    SerializedObject serializedDSL;

    SerializedProperty filename;
    SerializedProperty deletePreviousScene;

    private void OnEnable()
    {
        filename = serializedObject.FindProperty("filename");
        deletePreviousScene = serializedObject.FindProperty("deletePreviousScene");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        UnityDSL dsl = target as UnityDSL;

        EditorGUILayout.PropertyField(deletePreviousScene);
        EditorGUILayout.PropertyField(filename);
        if (GUILayout.Button("Parse From File")) dsl.ParseDSL();
        if (GUILayout.Button("Clear Scene")) dsl.ClearDSL();

        serializedObject.ApplyModifiedProperties();
    }
}
