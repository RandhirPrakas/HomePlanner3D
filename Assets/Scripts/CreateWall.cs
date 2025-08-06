using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateWall : MonoBehaviour
{
    public Vector3 p1, p2, d1;

    public float thickness, height;

    public Transform wall;
    public Material quadMaterial;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MapAllRequiredPoints(p1, p2);
        }
    }

    public void MapAllRequiredPoints(Vector3 p1, Vector3 p2)
    {
        Vector3 dir = (p2 - p1).normalized;
        Vector3 perp = new Vector3(-dir.z, 0, dir.x); // XZ plane perpendicular

        // Bottom rectangle
        Vector3 a = p1 + perp * (thickness / 2f);
        Vector3 d = p1 - perp * (thickness / 2f);
        Vector3 b = p2 + perp * (thickness / 2f);
        Vector3 c = p2 - perp * (thickness / 2f);

        // Top rectangle (add height in Y)
        Vector3 e = a + Vector3.up * height;
        Vector3 h = d + Vector3.up * height;
        Vector3 f = b + Vector3.up * height;
        Vector3 g = c + Vector3.up * height;

        // Draw and debug
        Debug.Log("<color=cyan>POINTS:</color>");
        Debug.Log($"A: {a}, B: {b}, C: {c}, D: {d}");
        Debug.Log($"E: {e}, F: {f}, G: {g}, H: {h}");

        // Generate faces (careful with winding order)
        // Floor (bottom face)
        GenerateQuads(a, d, c, b);

        // Ceiling (top face)
        GenerateQuads(e, f, g, h);

        // Front wall
        GenerateQuads(a, b, f, e);

        // Back wall
        GenerateQuads(d, h, g, c);

        // Left wall
        GenerateQuads(d, a, e, h);

        // Right wall
        GenerateQuads(b, c, g, f);
    }


    public void GenerateQuads(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Debug.Log("Generating Quad");
        GameObject quadObj = new GameObject("Wall");
        quadObj.transform.parent = wall;

        var mf = quadObj.AddComponent<MeshFilter>();
        var mr = quadObj.AddComponent<MeshRenderer>();
        mr.material = quadMaterial;

        var quadGen = quadObj.AddComponent<QuadGenerator>();
        quadGen.CreateQuad(p1, p2, p3, p4);
    }


    public Vector3 GetPerpendicularFromPoint(Vector3 point1, Vector3 point2, int side)
    {
        side = side < 0 ? -1 : 1;

        Vector3 dir = (point2 - point1).normalized;

        Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

        return point1 + perp * side * (thickness / 2f);
    }

    public Vector3 GetUpperPoints(Vector3 p)
    {
        p = new Vector3(p.x, p.y + height, p.z);
        return p;
    }
}
