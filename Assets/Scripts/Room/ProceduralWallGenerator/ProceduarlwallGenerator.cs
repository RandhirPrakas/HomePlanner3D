using System.Collections.Generic;
using UnityEngine;

public static class ProceduarlwallGenerator
{
    public static Material _quadMaterial;

    public static void Init()
    {
        _quadMaterial = Constants.DEFAULT_QUAD_MATERIAL;
        if (_quadMaterial == null)
        {
            Debug.LogError("Failed to load quad material from Resources.");
            return;
        }
        Debug.Log($"Quad mat = {_quadMaterial.name}");
    }


    public static List<GameObject> GenerateWallSegment(Vector3 p1, Vector3 p2, Wall mWall, float? height = null, float baseHeight = 0f, bool createCol = true)
    {
        float wallHeight = height ?? AppHelper._wallHeight;
        Transform wall = mWall.transform;

        float extensionAmount = AppHelper._wallThickness / 4f;
        Vector3 direction = (p2 - p1).normalized;

        //p1 -= direction * extensionAmount;
        //p2 += direction * extensionAmount;

        Vector3 dir = (p2 - p1).normalized;
        Vector3 perp = new Vector3(-dir.z, 0, dir.x);

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

        CreateWallCollider(p1, p2, mWall, wallHeight, baseHeight, createCol);

        return quads;
    }

    private static void CreateWallCollider(Vector3 p1, Vector3 p2, Wall wall, float wallHeight, float baseHeight, bool createCol = true)
    {
        if (!createCol)
            return;
        Vector3 mid = (p1 + p2) * 0.5f;
        float length = Vector3.Distance(p1, p2);

        GameObject colGO = new GameObject("WallColliderSegment");
        colGO.transform.SetParent(wall.transform, false);
        colGO.tag = Constants.TAG_WALL;
        colGO.layer = LayerMask.NameToLayer(Constants.LAYER_WALL);
        wall.WallSegmentColliders.Add(colGO);

        Vector3 worldPos = new Vector3(mid.x, baseHeight + wallHeight / 2f, mid.z);
        colGO.transform.position = worldPos;

        Vector3 dir = (p2 - p1).normalized;
        dir.y = 0; // flatten so no tilt
        if (dir != Vector3.zero)
            colGO.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // Add box collider
        BoxCollider bc = colGO.AddComponent<BoxCollider>();
        bc.size = new Vector3(AppHelper._wallThickness, wallHeight, length);
        bc.center = Vector3.zero;
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

    public static void CombineChildMeshes(Wall parent, List<GameObject> children)
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
        if (parent._material == null)
        {
            parentMR.material = _quadMaterial;
        }
        else
            parentMR.material = parent._material;

        foreach (var child in children)
        {
            GameObject.DestroyImmediate(child);
        }
    }

}
