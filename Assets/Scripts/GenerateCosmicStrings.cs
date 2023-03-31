using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenerateCosmicStrings : MonoBehaviour
{
    public float GizmoSize = 0.05f;
    public int NumberOfPoints = 30;

    private List<Vector3> UniqueVertices;
    private List<List<Vector3>> Faces = new List<List<Vector3>>();
    private List<Vector3> vertices;
    private int FaceVertexCount = 4;
    public List<Vector3> RandomPoints = new List<Vector3>();
    private List<Vector3> CenterPoints = new List<Vector3>();



    public void GenerateRandomPoints()
    {
        FindFaces();
        print(Faces.Count);
        GetCenterOfFaces();
        RandomPointsOnCube(NumberOfPoints);
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

        UniqueVertices = gloablVertices.Distinct().ToList();

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

    private void RandomPointsOnCube(int numberOfPoints)
    {
        RandomPoints.Clear();
        for (int i = 0; i < numberOfPoints; i++)
        {
            int faceIndex = Random.Range(0, Faces.Count);
            List<Vector3> face = Faces[faceIndex];
            int randomCornerId = Random.Range(0, 2) == 0 ? 0 : 2;
            float u = Random.Range(0, 1f);
            float v = Random.Range(0, 1f);
            if (v + u > 1)
            {
                v = 1 - v;
                u = 1 - u;
            }
            List<Vector3> edgeCorners = new List<Vector3>();

            edgeCorners.Add(face[3] - face[randomCornerId]);
            edgeCorners.Add(face[1] - face[randomCornerId]);

            Vector3 randomPoint = face[randomCornerId] + u * edgeCorners[0] + v * edgeCorners[1];
            RandomPoints.Add(randomPoint);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach (Vector3 point in RandomPoints)
        {
            if (gameObject.GetComponent<BoxCollider>().bounds.Contains(point))
                Gizmos.DrawSphere(point, GizmoSize);
        }
        Gizmos.color = Color.white;
        foreach (Vector3 center in CenterPoints)
        {
            Gizmos.DrawSphere(center, GizmoSize);
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
            print($"Point: {middlePoint} @ {i}");
            i++;
        }
    }


}
