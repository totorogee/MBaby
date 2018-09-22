using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Common.Track;

[CustomEditor(typeof(Train))]
public class TrainEditor : Editor
{
    public Train train;

    public override void OnInspectorGUI()
    {
       // base.OnInspectorGUI();

        serializedObject.Update();

        train = (Train)target;

        EditorGUILayout.Space();
        train.track = (Track)EditorGUILayout.ObjectField("Which Track to follow", train.track, typeof(Track), true);

        if (train.track != null)
        {
            EditorGUILayout.Space();
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("General Setting");
            EditorStyles.label.fontStyle = FontStyle.Normal;

            EditorGUI.indentLevel++;
            train.speed = EditorGUILayout.Slider("Speed (node/second)", train.speed, 1f, 1000f);
            if (train.track.nodes.Count == 0)
                EditorGUILayout.LabelField(" Set Track Data first !");
            else
                train.nodeNumber = (float) EditorGUILayout.IntSlider("Start At (node#)", (int)train.nodeNumber, 0, train.track.nodes.Count);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            string[] rotateOptions = new string[]
            { "Rotation not change", "Rotation follow track setting" };
            train.rotateSelected = EditorGUILayout.Popup(train.rotateSelected, rotateOptions);
            EditorStyles.label.fontStyle = FontStyle.Normal;

            if (train.rotateSelected != 0)
            {
                if (train.partsList.Count == 0)
                {
                    train.partsList.Add(new TrainParts { part = train.transform, facing = TrainDirection.Out });
                    serializedObject.Update();
                }
                ListParts("partsList");
            }

            EditorGUILayout.LabelField("Debug");
            if (GUILayout.Button("Stay/leave track")) train.OnTrackToggle();
        }

        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }

    public void ListParts(string path)
    {
        SerializedProperty listP = serializedObject.FindProperty(path);

        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Setting for Parts which rotate according to Track");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(60);
        EditorGUIUtility.labelWidth = 40f;
        EditorGUILayout.LabelField("Parts #");
        EditorGUILayout.LabelField("Prefab");
        EditorGUILayout.LabelField("Direction");
        EditorGUILayout.EndHorizontal();
        EditorStyles.label.fontStyle = FontStyle.Normal;

        for (int i = 0; i < listP.arraySize; i++)
        {
            SerializedProperty elementP = listP.GetArrayElementAtIndex(i);
            SerializedProperty pPart = elementP.FindPropertyRelative("part");
            SerializedProperty pFacing = elementP.FindPropertyRelative("facing");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("+", GUILayout.MaxWidth(20), GUILayout.MaxHeight(15)))
            {
                listP.InsertArrayElementAtIndex(i);
                break;
            }
            if (GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.MaxHeight(15)))
            {
                if (train.partsList.Count > 1)
                    listP.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUIUtility.labelWidth = 40f;
            EditorGUILayout.LabelField("Parts " + i);
            EditorGUILayout.PropertyField(pPart, new GUIContent(""));
            EditorGUILayout.PropertyField(pFacing, new GUIContent(""));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }
}
