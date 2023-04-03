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

        if(GUILayout.Button("Generate Curves Between Points"))
        {
            stringGenerator.ConnectPoints();
        }

        if(GUILayout.Button("Generate Galaxy Positions"))
        {
            stringGenerator.SpawnGalaxies();
        }

        if(GUILayout.Button("Delete Galaxies"))
        {
            stringGenerator.DeleteGalaxies();
        }

    }
}
