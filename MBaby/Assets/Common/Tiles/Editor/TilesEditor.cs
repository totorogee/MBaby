using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilesManager))]
public class TilesEditor : Editor {

    public bool showDebug = true;
    public bool switchWave = false;
    public int switchRange = 10;
    public bool disableWave = false;
    public int disableRange = 10;
    public bool bombWave = false;
    public int bombRange = 8;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TilesManager tm = (TilesManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Wave", EditorStyles.boldLabel);
        showDebug = EditorGUILayout.Toggle("Show Debug Wave", showDebug);
        if (showDebug)
        {
            EditorGUILayout.Space();
           
            tm.debugUnit = (Transform) EditorGUILayout.ObjectField("Wave Start at", tm.debugUnit, typeof(Transform), true);

            GUILayout.BeginHorizontal();
            switchWave = GUILayout.Button("Switch Wave", GUILayout.Width(120));
            switchRange = EditorGUILayout.IntSlider("Range", switchRange, 1,tm.maxRange*3);
            if (switchWave)
            {
                tm.StartWave(switchRange, tm.debugUnit.position, WaveChangeType.Switch);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            disableWave = GUILayout.Button("Disable Wave", GUILayout.Width(120));
            disableRange = EditorGUILayout.IntSlider("Range", disableRange, 1, tm.maxRange * 3);
            if (disableWave)
            {
                tm.StartWave(disableRange, tm.debugUnit.position, WaveChangeType.Off);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            bombWave = GUILayout.Button("Bomb Wave", GUILayout.Width(120));
            bombRange = EditorGUILayout.IntSlider("Range", bombRange, 3, tm.maxRange * 3);
            if (bombWave)
            {
                tm.StartWave(bombRange, tm.debugUnit.position, WaveChangeType.Bomb);
            }
            GUILayout.EndHorizontal();

        }
    }
}
