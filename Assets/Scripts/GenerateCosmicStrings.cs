using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public class GenerateCosmicStrings : MonoBehaviour
{
    public float GizmoSize = 0.05f;
    public int NumberOfPointPairs = 30;
    public int SubDivisions = 50;
    public float SpacingBetweenGalaxies = 0.05f;
    public float GalaxySize = 0.05f;
    public int NumberOfRandomPointsInsideCube = 10;

    [Header("Behavior Settings")]
    public bool ShouldPairStringOnOpposingSides = false;
    public int StringQuantizationDivisions = 10;
    public int DensityMultiplier = 1;

    [Header("Visibility Settings")]
    public bool ShouldDisplayStrings = false;
    public bool ShouldDispalyEndPoints = false;
    public bool ShouldDisplayCoordinatePoints = false;
    public bool ShouldDisplayFaceCenters = false;
    public bool ShouldDisplayGalaxies = true;


    private int FaceVertexCount = 4;
    private bool ShouldDrawLines = false;

    private List<Vector3> CornerVertices;
    private List<Vector3> vertices;
    private List<List<Vector3>> Faces = new List<List<Vector3>>();
    private List<EndPointPairs> RandomPoints = new List<EndPointPairs>();
    private List<Vector3> CenterPoints = new List<Vector3>();
    private List<Vector3> CoordinatePositions = new List<Vector3>();
    private List<BezierCurve> Strings = new List<BezierCurve>();
    private List<Vector3> RandomPointsInside = new List<Vector3>();
    public List<GameObject> Cubes = new List<GameObject>();
    public List<Vector3> QuantizedStringPositions = new List<Vector3>();
    public List<Vector3> GalaxyPositions = new List<Vector3>();

    public void GenerateEndPoints()
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
    public void ConnectPoints()
    {
        Strings.Clear();
        ShouldDrawLines = !ShouldDrawLines;
        foreach (EndPointPairs endPoint in RandomPoints)
        {
            Vector3[] points = { endPoint.StartPoint, Vector3.zero, endPoint.EndPoint };
            points[1] = RandomPointInCube(transform);
            BezierCurve curve = new BezierCurve(points);
            Strings.Add(curve);
        }
    }
    public void SpawnCoordinates()
    {
        CoordinatePositions = new List<Vector3>();
        Cubes.ForEach(item => DestroyImmediate(item));
        float localSpacing = gameObject.GetComponent<BoxCollider>().size.x / (SubDivisions + 1);
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
                    CoordinatePositions.Add(galaxyPosition);

                    if (x != SubDivisions && y != SubDivisions && z != SubDivisions)
                    {
                        Vector3 localPosition = transform.InverseTransformPoint(galaxyPosition);
                        float middlePoint = localSpacing / 2f;
                        Vector3 cubePosition = new Vector3(localPosition.x + middlePoint,
                                                           localPosition.y + middlePoint,
                                                           localPosition.z + middlePoint);

                        cubePosition = transform.TransformPoint(cubePosition);

                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = cubePosition;
                        cube.transform.rotation = transform.rotation;
                        cube.transform.localScale = new Vector3(localSpacing, localSpacing, localSpacing);
                        cube.GetComponent<MeshRenderer>().enabled = false;
                        cube.name = $"Cube_{x}_{y}_{z}";
                        Cubes.Add(cube);
                    }

                }
            }
        }
    }
    public void BuildUniverse()
    {
        DeleteUniverse();

        if (RandomPoints == null || RandomPoints.Count == 0)
        {
            GenerateEndPoints();
        }

        if (Strings == null || Strings.Count == 0)
        {
            ConnectPoints();
        }

        if (Cubes == null || Cubes.Count == 0)
        {
            SpawnCoordinates();
        }

        if (QuantizedStringPositions == null || QuantizedStringPositions.Count == 0)
        {
            MakeQuantizeStringPositions();
        }

        if (GalaxyPositions == null || GalaxyPositions.Count == 0)
        {
            GenerateGalaxies();
        }
    }


    public void DeleteCoordinates()
    {
        CoordinatePositions = new List<Vector3>();
        Cubes.ForEach(item => DestroyImmediate(item));
        Cubes.Clear();
    }
    public void DeleteRandomPointsInsideCube()
    {
        RandomPointsInside.Clear();
    }
    public void DeleteUniverse()
    {
        RandomPoints.Clear();
        Strings.Clear();
        DeleteCoordinates();
        QuantizedStringPositions.Clear();
        GalaxyPositions.Clear();
        RandomPointsInside.Clear();
    }


    public void GenerateGalaxies()
    {
        GalaxyPositions.Clear();
        foreach (GameObject cube in Cubes)
        {
            foreach (Vector3 stringPos in QuantizedStringPositions)
            {
                if (cube.GetComponent<BoxCollider>().bounds.Contains(stringPos))
                {
                    for (int i = 0; i < DensityMultiplier; i++)
                    {
                        GalaxyPositions.Add(RandomPointInCube(cube.transform));
                    }
                }
            }
        }
    }

    public void MakeQuantizeStringPositions()
    {
        QuantizedStringPositions.Clear();
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        foreach (BezierCurve curve in Strings)
        {
            List<Vector3> curvePoints = curve.GetSegments(StringQuantizationDivisions).ToList();
            QuantizedStringPositions.AddRange(curvePoints);
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
    private void GetCenterOfFaces()
    {
        int i = 0;
        GameObject[] centers = GameObject.FindGameObjectsWithTag("center");

        foreach (GameObject center in centers)
        {
                DestroyImmediate(center);
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
    private Vector3 RandomPointInCube(Transform cubeTransform)
    {
        BoxCollider collider = cubeTransform.gameObject.GetComponent<BoxCollider>();
        Vector3 halfSize = collider.size / 2f;
        float x = UnityEngine.Random.Range(-halfSize.x,halfSize.x);                 
        float y = UnityEngine.Random.Range(-halfSize.y,halfSize.y);                 
        float z = UnityEngine.Random.Range(-halfSize.z,halfSize.z);                 
        Vector3 newPoint = new Vector3(x, y, z);
        return cubeTransform.TransformPoint(newPoint);
    }
    public void GenerateRandomPointsInsideCube()
    {
        RandomPointsInside.Clear();

        

        for (int i = 0; i < NumberOfRandomPointsInsideCube; i++)
        {
            RandomPointsInside.Add(RandomPointInCube(transform));
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

    public void OnDrawGizmos()
    {
        if (ShouldDispalyEndPoints)
        {
            foreach (EndPointPairs point in RandomPoints)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(point.StartPoint, GizmoSize);
                Gizmos.color = Color.blue + Color.yellow;
                Gizmos.DrawCube(point.EndPoint, Vector3.one * 0.025f);
            }
        }


        if (ShouldDisplayFaceCenters)
        {
            Gizmos.color = Color.white;
            foreach (Vector3 center in CenterPoints)
            {
                Gizmos.DrawSphere(center, GizmoSize);
            }
        }

        if (ShouldDisplayStrings)
        {
            Gizmos.color = Color.green;
            foreach (BezierCurve curve in Strings)
            {
                Vector3[] points = curve.GetSegments(10);
                points.Skip(1).ToList().ForEach(p => Gizmos.DrawLine(points[Array.IndexOf(points, p) - 1], p));
            }
        }


        Gizmos.color = Color.magenta;
        if (ShouldDisplayCoordinatePoints)
        {
            foreach (Vector3 pos in CoordinatePositions)
            {
                Gizmos.DrawSphere(pos, GalaxySize);
            }
        }

        Gizmos.color = Color.cyan;
        foreach (Vector3 randomPoint in RandomPointsInside)
        {
            Gizmos.DrawSphere(randomPoint, GalaxySize);
        }

        if (ShouldDisplayGalaxies)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector3 galaxy in GalaxyPositions)
            {
                Gizmos.DrawSphere(galaxy, GalaxySize);
            }
        }

        Gizmos.color = Color.red;
        foreach (Vector3 stringPos in QuantizedStringPositions)
        {
            Gizmos.DrawSphere(stringPos, GalaxySize);
        }

    }

}

