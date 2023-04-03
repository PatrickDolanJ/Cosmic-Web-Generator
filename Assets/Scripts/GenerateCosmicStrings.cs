using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Net;

public class GenerateCosmicStrings : MonoBehaviour
{
    public float GizmoSize = 0.05f;
    public int NumberOfPointPairs = 30;
    public bool ShouldPairStringOnOpposingSides;
    public int SubDivisions = 50;
    public float SpacingBetweenGalaxies = 0.05f;
    public float GalaxySize = 0.05f;

    private List<Vector3> CornerVertices;
    private List<List<Vector3>> Faces = new List<List<Vector3>>();
    private List<Vector3> vertices;
    private int FaceVertexCount = 4;
    public List<EndPointPairs> RandomPoints = new List<EndPointPairs>();
    private List<Vector3> CenterPoints = new List<Vector3>();
    private bool ShouldDrawLines = false;
    private List<Vector3> GalaxiePositions = new List<Vector3>();
    private List<BezierCurve>  Strings = new List<BezierCurve>();

    public void GenerateRandomPoints()
    {
        FindFaces();
        print(Faces.Count);
        GetCenterOfFaces();
        RandomPoints.Clear();
        for (int i = 0; i < NumberOfPointPairs; i++)
        {
            RandomPoints.Add(MakeEndPoints(UnityEngine.Random.Range(0, Faces.Count)));
        }
    }

    private void FindFaces()
    {
        Faces.Clear();
        MeshFilter cube = gameObject.GetComponent<MeshFilter>();
        vertices = new List<Vector3>(cube.sharedMesh.vertices);
        List<Vector3> gloablVertices = new List<Vector3>();

        foreach (Vector3 vert in vertices)
        {
            gloablVertices.Add(transform.TransformPoint(vert));
        }

        CornerVertices = gloablVertices.Distinct().ToList();

        List<Vector3> oneFace = new List<Vector3>();
        int numOfVertices = gloablVertices.Count;
        for (int i = 0; i < numOfVertices; i++)
        {
            oneFace.Add(gloablVertices[i]);
            if ((i + 1) % FaceVertexCount == 0)
            {
                Faces.Add(oneFace);
                oneFace = new List<Vector3>();
            }
        }
    }

    private Vector3 RandomPointsOnCube(int faceIndex)
    {
        List<Vector3> face = Faces[faceIndex];
        int randomCornerId = UnityEngine.Random.Range(0, 2) == 0 ? 0 : 2;
        randomCornerId = 0;
        float u = UnityEngine.Random.Range(0, 1f);
        float v = UnityEngine.Random.Range(0, 1f);
        if (v + u > 1)
        {
            v = 1 - v;
            u = 1 - u;
        }
        List<Vector3> edgeCorners = new List<Vector3>();

        edgeCorners.Add(face[3] - face[randomCornerId]);
        edgeCorners.Add(face[1] - face[randomCornerId]);

        Vector3 randomPoint = face[randomCornerId] + u * edgeCorners[0] + v * edgeCorners[1];
        return randomPoint;
    }

    private EndPointPairs MakeEndPoints(int faceIndex)
    {
        int opposingFaceIndex;
        if (ShouldPairStringOnOpposingSides)
        {
            opposingFaceIndex = faceIndex % 2 == 0 ? faceIndex + 1 : faceIndex - 1;
        }
        else
        {
            opposingFaceIndex = UnityEngine.Random.Range(0, Faces.Count);
            while (opposingFaceIndex == faceIndex)
            {
                opposingFaceIndex = UnityEngine.Random.Range(0, Faces.Count);
            }
        }
        Vector3 startPoint = RandomPointsOnCube(faceIndex);
        Vector3 endPoint = RandomPointsOnCube(opposingFaceIndex);
        return new EndPointPairs(startPoint, endPoint);
    }



    public void OnDrawGizmos()
    {
        foreach (EndPointPairs point in RandomPoints)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(point.StartPoint, GizmoSize);
            Gizmos.color = Color.blue + Color.yellow;
            Gizmos.DrawCube(point.EndPoint, Vector3.one * 0.025f);
        }
        Gizmos.color = Color.white;
        foreach (Vector3 center in CenterPoints)
        {
            Gizmos.DrawSphere(center, GizmoSize);
        }
        if (ShouldDrawLines)
        {
            Gizmos.color = Color.green;
            foreach (BezierCurve curve in Strings)
            {
                Vector3[] points = curve.GetSegments(10);
                points.Skip(1).ToList().ForEach(p => Gizmos.DrawLine(points[Array.IndexOf(points, p) - 1], p));
            }
        }

        Gizmos.color = Color.magenta;

        foreach (Vector3 galaxy in GalaxiePositions)
        {
            Gizmos.DrawSphere(galaxy, GalaxySize);
        }

    }

    public void ConnectPoints()
    {
        ShouldDrawLines = !ShouldDrawLines;
        foreach(EndPointPairs endPoint in RandomPoints)
        {
            Vector3[] points = {endPoint.StartPoint, Vector3.zero,endPoint.EndPoint};
            points[1] = transform.position;
            BezierCurve curve = new BezierCurve(points);
            Strings.Add(curve);
        }
    }

    private void GetCenterOfFaces()
    {
        int i = 0;
        GameObject[] centers = GameObject.FindGameObjectsWithTag("center");

        foreach (GameObject center in centers)
        {
            if (center != null)
            {
                DestroyImmediate(center);
            }
        }

        foreach (List<Vector3> face in Faces)
        {
            Vector3 middlePoint = (face[0] + face[1] + face[2] + face[3]) / 4;
            CenterPoints.Add(middlePoint);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = middlePoint;
            sphere.name = i.ToString();
            sphere.transform.localScale = Vector3.one * 0.004f;
            sphere.tag = "center";
            i++;
        }
    }

    public void SpawnGalaxies()
    {
        GalaxiePositions = new List<Vector3>();
        float localSpacing = gameObject.GetComponent<BoxCollider>().size.x / SubDivisions;
        float halfOfDivisions = localSpacing * (SubDivisions / 2f);

        Vector3 startVector = transform.position;
        startVector = transform.InverseTransformPoint(startVector);

        startVector.x -= halfOfDivisions;
        startVector.y -= halfOfDivisions;
        startVector.z -= halfOfDivisions;


        for (int x = 0; x <= SubDivisions; x++)
        {
            for (int y = 0; y <= SubDivisions; y++)
            {
                for (int z = 0; z <= SubDivisions; z++)
                {
                    Vector3 galaxyPosition = new Vector3(
                        x * localSpacing,
                        y * localSpacing,
                        z * localSpacing
                        );
                    galaxyPosition += startVector;
                    galaxyPosition = transform.TransformPoint(galaxyPosition);
                    GalaxiePositions.Add(galaxyPosition);
                }
            }
        }



    }

    public void DeleteGalaxies()
    {
        GalaxiePositions = new List<Vector3>();
    }

}

