using System.Collections.Generic;
using UnityEngine;

public static class ProceduarlwallGenerator
{
    public static Material _quadMaterial;

    public static void Init()
    {
        _quadMaterial = Resources.Load<Material>("ProceduralMaterials/QuadMaterial");
        if (_quadMaterial == null)
        {
            Debug.LogError("Failed to load quad material from Resources.");
            return;
        }
        Debug.Log($"Quad mat = {_quadMaterial.name}");
    }

    //Original
    /*public void GenerateWallAndOpening(Vector3 p1, Vector3 p2, Transform wall)
    {
        float extensionAmount = AppHelper._wallThickness / 4f;
        Vector3 direction = (p2 - p1).normalized;

        p1 -= direction * extensionAmount;
        p2 += direction * extensionAmount;

        Vector3 dir = (p2 - p1).normalized;
        Vector3 perp = new Vector3(-dir.z, 0, dir.x); // XZ plane perpendicular

        // Bottom rectangle
        Vector3 a = p1 + perp * (AppHelper._wallThickness / 2f);
        Vector3 d = p1 - perp * (AppHelper._wallThickness / 2f);
        Vector3 b = p2 + perp * (AppHelper._wallThickness / 2f);
        Vector3 c = p2 - perp * (AppHelper._wallThickness / 2f);

        // Top rectangle (add Wall Height in Y)
        Vector3 e = a + Vector3.up * AppHelper._wallHeight;
        Vector3 h = d + Vector3.up * AppHelper._wallHeight;
        Vector3 f = b + Vector3.up * AppHelper._wallHeight;
        Vector3 g = c + Vector3.up * AppHelper._wallHeight;

        // Draw and debug
       *//* Debug.Log("<color=cyan>POINTS:</color>");
        Debug.Log($"A: {a}, B: {b}, C: {c}, D: {d}");
        Debug.Log($"E: {e}, F: {f}, G: {g}, H: {h}");*//*

        // Always Retain this points Order, this creates the wall in single direction (here Clockwise) so that the quads always faces outward
        List<GameObject> quads = new List<GameObject>();

        quads.Add(GenerateQuads(a, d, c, b, wall)); // bottom
        quads.Add(GenerateQuads(e, f, g, h, wall)); // top
        quads.Add(GenerateQuads(a, b, f, e, wall)); // front
        quads.Add(GenerateQuads(d, h, g, c, wall)); // back
        quads.Add(GenerateQuads(d, a, e, h, wall)); // left
        quads.Add(GenerateQuads(b, c, g, f, wall)); // right

        CombineChildMeshes(wall, quads);
    }*/


    public static List<GameObject> GenerateWallSegment(Vector3 p1, Vector3 p2, Transform wall, float? height = null, float baseHeight = 0f)
    {
        float wallHeight = height ?? AppHelper._wallHeight;

        float extensionAmount = AppHelper._wallThickness / 4f;
        Vector3 direction = (p2 - p1).normalized;

        p1 -= direction * extensionAmount;
        p2 += direction * extensionAmount;

        Vector3 dir = (p2 - p1).normalized;
        Vector3 perp = new Vector3(-dir.z, 0, dir.x); // XZ plane perpendicular

        // Bottom rectangle (shifted up by baseHeight)
        Vector3 a = p1 + perp * (AppHelper._wallThickness / 2f) + Vector3.up * baseHeight;
        Vector3 d = p1 - perp * (AppHelper._wallThickness / 2f) + Vector3.up * baseHeight;
        Vector3 b = p2 + perp * (AppHelper._wallThickness / 2f) + Vector3.up * baseHeight;
        Vector3 c = p2 - perp * (AppHelper._wallThickness / 2f) + Vector3.up * baseHeight;

        // Top rectangle
        Vector3 e = a + Vector3.up * wallHeight;
        Vector3 h = d + Vector3.up * wallHeight;
        Vector3 f = b + Vector3.up * wallHeight;
        Vector3 g = c + Vector3.up * wallHeight;

        // Convert to local space
        a = wall.InverseTransformPoint(a);
        b = wall.InverseTransformPoint(b);
        c = wall.InverseTransformPoint(c);
        d = wall.InverseTransformPoint(d);
        e = wall.InverseTransformPoint(e);
        f = wall.InverseTransformPoint(f);
        g = wall.InverseTransformPoint(g);
        h = wall.InverseTransformPoint(h);

        List<GameObject> quads = new List<GameObject>();

        quads.Add(GenerateQuads(a, d, c, b, wall));
        quads.Add(GenerateQuads(e, f, g, h, wall));
        quads.Add(GenerateQuads(a, b, f, e, wall));
        quads.Add(GenerateQuads(d, h, g, c, wall));
        quads.Add(GenerateQuads(d, a, e, h, wall));
        quads.Add(GenerateQuads(b, c, g, f, wall));

        return quads;
    }



    public static GameObject GenerateQuads(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Transform wall)
    {
        GameObject quadObj = new GameObject("WallPart");
        quadObj.transform.parent = wall;

        var mf = quadObj.AddComponent<MeshFilter>();
        var mr = quadObj.AddComponent<MeshRenderer>();
        mr.material = _quadMaterial;

        var quadGen = quadObj.AddComponent<QuadGenerator>();
        quadGen.CreateQuad(p1, p2, p3, p4);

        return quadObj;
    }

    public static void CombineChildMeshes(Transform parent, List<GameObject> children)
    {
        List<CombineInstance> combine = new List<CombineInstance>();

        foreach (var child in children)
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf == null) continue;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = mf.transform.localToWorldMatrix;

            combine.Add(ci);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine.ToArray());

        var parentMF = parent.GetComponent<MeshFilter>();
        if (parentMF == null) parentMF = parent.gameObject.AddComponent<MeshFilter>();
        
        var parentMR = parent.GetComponent<MeshRenderer>();
        if (parentMR == null) parentMR = parent.gameObject.AddComponent<MeshRenderer>();

        parentMF.sharedMesh = combinedMesh;
        parentMR.material = _quadMaterial;

        foreach (var child in children)
        {
            GameObject.DestroyImmediate(child);
        }
    }

}
