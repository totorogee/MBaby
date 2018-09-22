using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Common.Shooter;

[CustomEditor(typeof(Hp))]
public class HpEditor : Editor
{
    public Hp hp;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        hp = (Hp)target;
        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug");
        if (GUILayout.Button("Update Setting"))
        {
            hp.UpdateOffset(true); 
        }

        serializedObject.ApplyModifiedProperties();
    }
}