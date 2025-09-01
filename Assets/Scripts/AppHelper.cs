
// Will Contain wrapper, calculations, some unique feature which will be used later etcs
using System;
using System.Collections.Generic;
using UnityEngine;

public static class AppHelper
{
    #region Variables_ProceduralWallGeneration

    public static readonly float _minimumWallLength = 4f;
    public static readonly float _minimumWallHeight = 5f;
    public static readonly float _wallThickness = 1f;
    public static readonly float _wallHeight = 7f;

    #endregion

    #region Variables_PointsManagements

    public static readonly float _pointSnapThreshold = 5f;
    public static readonly float _nearestWallSnapThreshold = 3f;

    #endregion


    #region Events

    

    #region Invoker Functions
    
    #endregion

    #endregion


    public static readonly float _lrYPos = 0.1f;
    public static readonly float _lrThickness = 0.5f;

    // this check if distance between two point is 
    public static bool CanSnapPoint(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < _pointSnapThreshold;
    }

    // pointToSnap will be snapped to snapPosition
    public static Vector3 SnapPoint(Vector3 snapPosition, Vector3 pointToSnap)
    {
        if (CanSnapPoint(snapPosition, pointToSnap))
        {
            pointToSnap = snapPosition;
        }
        return pointToSnap;
    }

    public static Vector3 SmartSnapToAxis(Vector3 currentPosition, List<WallPoint> allWallPoints)
    {
        float closestXDiff = float.MaxValue;
        float closestZDiff = float.MaxValue;
        float? snapX = null;
        float? snapZ = null;

        foreach (var wp in allWallPoints)
        {
            float xDiff = Mathf.Abs(currentPosition.x - wp._position.x);
            float zDiff = Mathf.Abs(currentPosition.z - wp._position.z);

            if (xDiff < closestXDiff)
            {
                closestXDiff = xDiff;
                snapX = wp._position.x;
            }

            if (zDiff < closestZDiff)
            {
                closestZDiff = zDiff;
                snapZ = wp._position.z;
            }
        }

        if (closestXDiff < _pointSnapThreshold)
        {
            currentPosition.x = snapX.Value;
        }

        if (closestZDiff < _pointSnapThreshold)
        {
            currentPosition.z = snapZ.Value;
        }

        return currentPosition;
    }

    public static float DistanceBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    } 

    public static float DistanceBetweenTwoPoints(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }


    public static Vector3 WrapPosition(Vector3 startPosition, Vector3 endPosition)
    {
        if (Mathf.Abs(startPosition.x - endPosition.x) < _pointSnapThreshold)
        {
            endPosition = new Vector3(startPosition.x, endPosition.y, endPosition.z);
        }
        else if (Mathf.Abs(startPosition.z - endPosition.z) < _pointSnapThreshold)
        {
            endPosition = new Vector3(endPosition.x, endPosition.y, startPosition.z);
        }

        return endPosition;
    }

    // Calculate area from the list of Vector3 points
    public static float CalculatePolygonArea(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
            return 0f;

        float area = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % points.Count];

            // Use XZ plane for area (like floor plan)
            area += (p1.x * p2.z) - (p2.x * p1.z);
        }

        return Mathf.Abs(area) * 0.5f;
    }

    // Calculate area from the list of Vector2 points
    public static float CalculatePolygonArea(List<Vector2> points)
    {
        if (points == null || points.Count < 3)
            return 0f;

        float area = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];

            area += (p1.x * p2.y) - (p2.x * p1.y);
        }

        return Mathf.Abs(area) * 0.5f;
    }

    public static bool TryGetLineIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 intersection)
    {
        // Project to XZ plane (assuming y is height)
        Vector2 a = new Vector2(A.x, A.z);
        Vector2 b = new Vector2(B.x, B.z);
        Vector2 c = new Vector2(C.x, C.z);
        Vector2 d = new Vector2(D.x, D.z);

        intersection = Vector3.zero;

        Vector2 r = b - a;
        Vector2 s = d - c;

        float denominator = r.x * s.y - r.y * s.x;

        if (Mathf.Abs(denominator) < Mathf.Epsilon)
        {
            // Lines are parallel or collinear
            return false;
        }

        Vector2 cma = c - a;
        float t = (cma.x * s.y - cma.y * s.x) / denominator;
        float u = (cma.x * r.y - cma.y * r.x) / denominator;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            // Intersection point in 2D
            Vector2 inter2D = a + t * r;
            intersection = new Vector3(inter2D.x, A.y, inter2D.y); // Keep Y from A (or adjust as needed)
            return true;
        }

        return false; // No intersection within the line segments
    }

    public static bool IsPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p, float tolerance = 0.001f)
    {

        Vector3 ab = b - a;
        Vector3 ap = p - a;

        
        if (Vector3.Cross(ab, ap).sqrMagnitude > tolerance)
            return false;

        float dot = Vector3.Dot(ap, ab);
        if (dot < 0) return false;

        if (dot > ab.sqrMagnitude) return false;

        return true;
    }
}
