using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGenerator : MonoBehaviour
{
    public void CreateQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4] { p1, p2, p3, p4 };

        // Ensure triangles use clockwise winding (0->1->2 and 2->3->0)
        int[] triangles = new int[6] { 0, 1, 2, 2, 3, 0 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }


    public Mesh GenerateFloor(List<Vector3> inputVertices)
    {
        if (inputVertices == null || inputVertices.Count < 3)
        {
            Debug.LogError("At least 3 points are required to generate a mesh.");
            return null;
        }


        List<Vector2> flatVerts2D = new List<Vector2>();
        foreach (var v in inputVertices)
        {
            flatVerts2D.Add(new Vector2(v.x, v.z));
        }


        List<int> triangleIndices = Triangulate(flatVerts2D);

        if (triangleIndices == null || triangleIndices.Count < 3)
        {
            Debug.LogError("Triangulation failed.");
            return null;
        }


        Mesh mesh = new Mesh();
        mesh.SetVertices(inputVertices);
        mesh.SetTriangles(triangleIndices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private List<int> Triangulate(List<Vector2> points)
    {
        int n = points.Count;
        if (n < 3)
            return null;

        List<int> indices = new List<int>();
        List<int> V = new List<int>();

        // Clockwise is front-facing in Unity (Y+)
        if (Area(points) < 0) // Already clockwise
        {
            for (int i = 0; i < n; i++) V.Add(i);
        }
        else // Reverse to make it clockwise
        {
            for (int i = 0; i < n; i++) V.Add(n - 1 - i);
        }

        int nv = V.Count;
        int count = 0;
        int v = nv - 1;

        while (nv > 2)
        {
            if ((count++) > 1000)
            {
                Debug.LogError("Triangulation infinite loop prevention triggered.");
                return null;
            }

            int u = v; if (nv <= u) u = 0;
            v = u + 1; if (nv <= v) v = 0;
            int w = v + 1; if (nv <= w) w = 0;

            if (IsEar(u, v, w, V, points))
            {
                indices.Add(V[u]);
                indices.Add(V[v]);
                indices.Add(V[w]);

                V.RemoveAt(v);
                nv--;
                count = 0;
            }
        }

        return indices;
    }

    //Calculates signed area to determine winding order.
    private float Area(List<Vector2> points)
    {
        float area = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];
            area += (p1.x * p2.y) - (p2.x * p1.y);
        }
        return area * 0.5f;
    }


    //Determines if triangle formed by u,v,w is an ear.
   
    private bool IsEar(int u, int v, int w, List<int> V, List<Vector2> points)
    {
        Vector2 A = points[V[u]];
        Vector2 B = points[V[v]];
        Vector2 C = points[V[w]];

        if (!IsConvex(A, B, C))
            return false;

        for (int i = 0; i < V.Count; i++)
        {
            if (i == u || i == v || i == w)
                continue;

            if (PointInTriangle(points[V[i]], A, B, C))
                return false;
        }

        return true;
    }

    private bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 ab = b - a;
        Vector2 bc = c - b;
        return (ab.x * bc.y - ab.y * bc.x) < 0f; // clockwise test
    }

    //Checks if point P is inside triangle ABC.
    private bool PointInTriangle(Vector2 P, Vector2 A, Vector2 B, Vector2 C)
    {
        float area = 0.5f * (-B.y * C.x + A.y * (-B.x + C.x) + A.x * (B.y - C.y) + B.x * C.y);
        float s = 1 / (2 * area) * (A.y * C.x - A.x * C.y + (C.y - A.y) * P.x + (A.x - C.x) * P.y);
        float t = 1 / (2 * area) * (A.x * B.y - A.y * B.x + (A.y - B.y) * P.x + (B.x - A.x) * P.y);
        float u = 1 - s - t;

        return s > 0 && t > 0 && u > 0;
    }
}
