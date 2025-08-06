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
}
