using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GeometryUtils
{
    // Gift wrapping convex hull in XZ plane
    public static List<Vector3> ComputeConvexHullXZ(List<Vector3> points)
    {
        if (points == null || points.Count < 3) return new List<Vector3>(points);

        // Work in 2D (XZ plane)
        List<Vector2> pts2D = points.Select(p => new Vector2(p.x, p.z)).ToList();

        int leftMost = 0;
        for (int i = 1; i < pts2D.Count; i++)
        {
            if (pts2D[i].x < pts2D[leftMost].x)
                leftMost = i;
        }

        List<int> hullIndices = new List<int>();
        int p = leftMost;

        do
        {
            hullIndices.Add(p);
            int q = (p + 1) % pts2D.Count;

            for (int r = 0; r < pts2D.Count; r++)
            {
                if (Orientation(pts2D[p], pts2D[r], pts2D[q]) < 0)
                    q = r;
            }

            p = q;

        } while (p != leftMost);

        return hullIndices.Select(i => points[i]).ToList();
    }

    // Orientation test: <0 = clockwise turn, >0 = counter-clockwise, 0 = collinear
    private static float Orientation(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }
}
