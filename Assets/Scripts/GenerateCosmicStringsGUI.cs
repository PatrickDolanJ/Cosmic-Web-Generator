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

        GUILayout.Space(20);

        GenerateCosmicStrings stringGenerator = (GenerateCosmicStrings)target;

        if (GUILayout.Button("Generate EndPoints for Strings"))
        {
            stringGenerator.GenerateEndPoints();
        }

        if(GUILayout.Button("Generate Strings Between EndPoints"))
        {
            stringGenerator.ConnectPoints();
        }

        if(GUILayout.Button("Generate Coordinate Objects"))
        {
            stringGenerator.SpawnCoordinates();
        }


        if(GUILayout.Button("Generate Random Points Inside Cube"))
        {
            stringGenerator.GenerateRandomPointsInsideCube();
        }
        
        if(GUILayout.Button("Quantize Strings"))
        {
            stringGenerator.MakeQuantizeStringPositions();
        }

        if(GUILayout.Button("Spawn Galaxies"))
        {
            stringGenerator.GenerateGalaxies();
        }

        if(GUILayout.Button("Build A Universe"))
        {
            stringGenerator.BuildUniverse();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Delete Coordinate Objects"))
        {
            stringGenerator.DeleteCoordinates();
        }

        if(GUILayout.Button("Delete Random Points in Cube"))
        {
            stringGenerator.DeleteRandomPointsInsideCube();
        }

        if(GUILayout.Button("Destroy Universe"))
        {
            stringGenerator.DeleteUniverse();
        }
    }
}
