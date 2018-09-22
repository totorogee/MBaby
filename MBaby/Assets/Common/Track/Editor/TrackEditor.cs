using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Common.Track;

[CustomEditor(typeof(Track))]
public class TrackEditor : Editor
{
    public Track track;
    public Vector2 v2 = Vector2.zero;

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        track = (Track)target;
        serializedObject.Update();

        if (track.sessions.Count == 0)
        {
            track.sessions.Add(new Session());
            serializedObject.Update();
        }

        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("General Setting");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;
        track.filpNormal = EditorGUILayout.Toggle("Filp Direction ",track.filpNormal);
        track.autoFinish = EditorGUILayout.Toggle("Auto Finish", track.autoFinish);
        string[] posOptions = new string[]
        { "Center","1","2","3","4","5","6","7","8","9" };
        track.positionSelected = EditorGUILayout.Popup("Position (Numpad)",track.positionSelected, posOptions);

        track.lenghtOfNode = EditorGUILayout.Slider("Lenght of Node", track.lenghtOfNode, 0.05f,0.5f);
        EditorGUILayout.Space();
        ListSession("sessions");
        EditorGUI.indentLevel--;

        if (GUILayout.Button("Update Track"))
            track.DebugShowNodes();
        EditorGUILayout.LabelField("Node used : " + track.nodes.Count);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug");

        v2 = EditorGUILayout.Vector2Field("v2", v2);

        if (GUILayout.Button("Nearest"))
        {
            Debug.Log(track.NearestNode(v2));
        }

        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }

    public void ListSession(string path)
    {
        SerializedProperty listS = serializedObject.FindProperty(path);
        EditorGUI.indentLevel++;

        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(60);
        EditorGUIUtility.labelWidth = 40f;
        EditorGUILayout.LabelField("Session #");
        EditorGUILayout.LabelField("Lenght");
        EditorGUILayout.LabelField("Type");
        EditorGUILayout.LabelField("Direction");
        EditorGUILayout.EndHorizontal();
        EditorStyles.label.fontStyle = FontStyle.Normal;

        for (int i = 0; i < listS.arraySize; i++)
        {
            SerializedProperty elementS = listS.GetArrayElementAtIndex(i);
            SerializedProperty sLenght = elementS.FindPropertyRelative("lenght");
            SerializedProperty sType = elementS.FindPropertyRelative("moveType");
            SerializedProperty sDir = elementS.FindPropertyRelative("direction");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("+", GUILayout.MaxWidth(20), GUILayout.MaxHeight(15)))
            {
                listS.InsertArrayElementAtIndex(i);
                break;
            }
            if (GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.MaxHeight(15)))
            {
                if (track.sessions.Count > 1)
                    listS.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUIUtility.labelWidth = 40f;
            EditorGUILayout.LabelField("Session " + i);
            EditorGUILayout.PropertyField(sLenght, new GUIContent(""));
            EditorGUILayout.PropertyField(sType, new GUIContent(""));
            EditorGUILayout.PropertyField(sDir, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

        }
        EditorGUI.indentLevel--;
    }

}
