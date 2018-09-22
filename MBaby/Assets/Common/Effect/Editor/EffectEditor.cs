using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Effect))]
public class EffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        Effect eff = (Effect)target;

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUIUtility.labelWidth = 130f;

        string[] applyOptions = new string[]
        { "Apply effect to This Object Only ", "Apply effect to others" };
        eff.applySelected = EditorGUILayout.Popup(eff.applySelected, applyOptions);
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;

        if (eff.applySelected !=0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("srList"), new GUIContent("Sprite Renderer (Color)"),true );
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rList"), new GUIContent("Renderer (Color) "), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tList"), new GUIContent("Transfrom (Size)"), true);
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.indentLevel--;


        #region Size Related Setting

        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUIUtility.labelWidth = 130f;
        string[] sizeOptions = new string[]
        { "Constant Size", "Change Size" };
        eff.sizeSelected = EditorGUILayout.Popup("Size/Time Setting", eff.sizeSelected, sizeOptions);
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;

        eff.stayTime = EditorGUILayout.Slider("Effect Duration ", eff.stayTime, 0f, 20f);
        eff.keepStay = EditorGUILayout.Toggle("Stay after effect ", eff.keepStay);

        string[] timeOptions = new string[]
        { "Play Once", "PingPong", "Repeat" };
        if (eff.keepStay)
        {
            eff.timeSelected = EditorGUILayout.Popup("Repeat Effect", eff.timeSelected, timeOptions);
        }

        if (eff.sizeSelected != 0)
        {
            if (eff.sizeCurve.length <= 1)
            {
                eff.sizeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
            }
            eff.sizeCurve = EditorGUILayout.CurveField("Size/Time Curve", eff.sizeCurve);
        }

        EditorGUI.indentLevel--;

        #endregion

        #region Color Related Setting

        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUIUtility.labelWidth = 130f;

        string[] colorOptions = new string[]
        { "Disable", "Set End Alpha", "Set End Color" };

        eff.colorSelected = EditorGUILayout.Popup("Color Setting", eff.colorSelected, colorOptions);
        EditorStyles.label.fontStyle = FontStyle.Normal;

        if (eff.colorSelected > 0)
        {
            EditorGUI.indentLevel++;

            if (eff.colorSelected == 1)
                eff.endAlpha = EditorGUILayout.Slider("End Alpha", eff.endAlpha, 0f, 1f);
            else
                eff.endColor = EditorGUILayout.ColorField("End Color", eff.endColor);

            GUILayout.BeginHorizontal();
            if (eff.colorCurve.length <= 1)
            {
                eff.colorCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
            }
            GUILayout.EndHorizontal();

            eff.colorCurve = EditorGUILayout.CurveField("Color/Time Curve", eff.colorCurve);
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Follow Related Setting

        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 130f;
        EditorStyles.label.fontStyle = FontStyle.Bold;
        string[] followOptions = new string[]
        { "Disable", "Fixed offset", "Fixed distant", "Fixed distnat and Area", "Rotate Around" };

        eff.followSelected = EditorGUILayout.Popup("Following Setting", eff.followSelected, followOptions);
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;
        if (eff.followSelected == 0)
            eff.followSpeedSelected = 0;

        string[] followSpeedOptions = new string[]
        { "Instant", "Fixed speed", "Ratio per second" };
        EditorGUIUtility.labelWidth = 130f;
        switch (eff.followSelected)
        {
            case 0:
                break;
            case 1: // Fixed offset
                eff.followOffset = EditorGUILayout.Vector2Field("Offset", eff.followOffset);
                eff.followTo = (Transform)EditorGUILayout.ObjectField("Following", eff.followTo, typeof(Transform), true);
                eff.followSpeedSelected = EditorGUILayout.Popup("Speed Setting", eff.followSpeedSelected, followSpeedOptions);
                break;
            case 2: // Fixed distant
                eff.followDistant = EditorGUILayout.Slider("Distant", eff.followDistant, 0, 20f);
                eff.followTo = (Transform)EditorGUILayout.ObjectField("Following", eff.followTo, typeof(Transform), true);
                eff.followSpeedSelected = EditorGUILayout.Popup("Speed Setting", eff.followSpeedSelected, followSpeedOptions);
                break;
            case 3: // Fixed distant and Area
                eff.followDistant = EditorGUILayout.Slider("Distant", eff.followDistant, 0, 20f);
                eff.followStartArea = EditorGUILayout.IntSlider("Form (degree)" , eff.followStartArea, 0, 359);
                eff.followEndArea = EditorGUILayout.IntSlider("To (degree)", eff.followEndArea, 0, 359);
                eff.followTo = (Transform)EditorGUILayout.ObjectField("Following", eff.followTo, typeof(Transform), true);
                eff.followSpeedSelected = EditorGUILayout.Popup("Speed Setting", eff.followSpeedSelected, followSpeedOptions);
                break;
            case 4: // Rotate Around
                eff.followDistant = EditorGUILayout.Slider("Distant", eff.followDistant, 0, 20f);
                eff.followTo = (Transform)EditorGUILayout.ObjectField("Following", eff.followTo, typeof(Transform), true);
                eff.followSpeedSelected = 1;
                break;

            default:
                break;
        }

        switch (eff.followSpeedSelected)
        {
            case 0: // Instant
                break;

            case 1: // Fixed speed
                eff.followSpeed = EditorGUILayout.Slider("Speed", eff.followSpeed, 0f, 100f);
                break;

            case 2: // Ratio per second
                eff.followRatio = EditorGUILayout.Slider("Ratio", eff.followRatio, 0f, 100f);
                break;

            default: // Instant
                break;
        }

        EditorGUI.indentLevel--;

        #endregion

        #region Rotation Related Setting

        EditorGUILayout.Space();

        EditorStyles.label.fontStyle = FontStyle.Bold;
        string[] rotateOptions = new string[]
        { "Disable","Look At target", "Rotation by curve setting", "Same as Target" };

        eff.rotationSelected = EditorGUILayout.Popup("Rotation Setting", eff.rotationSelected, rotateOptions);
        EditorStyles.label.fontStyle = FontStyle.Normal;

        if (eff.sizeCurve.length <= 1)
        {
            eff.sizeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        }

        EditorGUI.indentLevel++;
        EditorGUIUtility.labelWidth = 130f;

        switch (eff.rotationSelected)
        {
            case 0: // disable
                break;

            case 1: // look at target

                eff.rotateTo = (Transform)EditorGUILayout.ObjectField("Look At", eff.rotateTo, typeof(Transform), true);
                
                eff.degPerSecond = EditorGUILayout.Slider("Degree pre second ", eff.degPerSecond, 0f, 1000f);
                break;

            case 2: // Start at Random

                eff.degPerSecond = EditorGUILayout.Slider("Peak Speed ", eff.degPerSecond, 0f, 3600f);

                EditorGUILayout.LabelField("Curve Settings", EditorStyles.boldLabel);
                eff.rotationCurve = EditorGUILayout.CurveField("Rotate/Time Curve", eff.rotationCurve);
                break;
            case 3: // Same as target

                eff.rotateTo = (Transform)EditorGUILayout.ObjectField("Same as ", eff.rotateTo, typeof(Transform), true);
                
                eff.degPerSecond = EditorGUILayout.Slider("Degree pre second ", eff.degPerSecond, 0f, 3600f);

                break;

            default:
                break;
        }

        EditorGUI.indentLevel--;
        #endregion

        #region Spawn Related Setting

        EditorGUILayout.Space();

        EditorStyles.label.fontStyle = FontStyle.Bold;
        string[] spawnOptions = new string[]
        { "Disable", "Spawn when Effect Ended", "Spawn when Kill" };
        eff.spawnSelected = EditorGUILayout.Popup("Spawn Setting", eff.spawnSelected, spawnOptions);
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUI.indentLevel++;
        switch (eff.spawnSelected)
        {
            case 0: // Disable
                break;

            case 1: // Spawn when Effect Ended
                eff.spawn = (Transform)EditorGUILayout.ObjectField("Spawn", eff.spawn, typeof(Transform), true);
                eff.spawnTime = EditorGUILayout.Slider("Early/Delay", eff.spawnTime, -0.99f, 0.99f);
                break;

            case 2: // Spawn when Kill
                eff.spawn = (Transform)EditorGUILayout.ObjectField("Spawn", eff.spawn, typeof(Transform), true);
                eff.spawnTime = EditorGUILayout.Slider("Delay(in seconds)", eff.spawnTime, 0f, 10f);
                break;

            default:
                break;
        }
        EditorGUI.indentLevel--;
        #endregion

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Debug");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("SpawnOnce")) eff.spawnOnce = true;
        if (GUILayout.Button("Hide")) eff.Hide();
        if (GUILayout.Button("Pause")) eff.Pause();
        if (GUILayout.Button("Kill")) eff.Kill();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate")) eff.NoRotate();
        if (GUILayout.Button("Follow")) eff.NoFollow();
        if (GUILayout.Button("Spawn")) eff.NoSpawn();
        EditorGUILayout.EndHorizontal();


    }
}