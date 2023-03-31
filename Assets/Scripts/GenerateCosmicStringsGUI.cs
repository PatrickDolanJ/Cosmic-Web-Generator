using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateCosmicStrings))]
public class GenerateCosmicStringsGUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GenerateCosmicStrings stringGenerator = (GenerateCosmicStrings)target;

        if (GUILayout.Button("Generate Random Points on Surface"))
        {
            stringGenerator.GenerateRandomPoints();
        }
    }
}
