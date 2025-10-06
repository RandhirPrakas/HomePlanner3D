
// Will Contain wrapper, calculations, some unique feature which will be used later etcs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class AppHelper
{
    #region Variables_ProceduralWallGeneration

    public static readonly float _minimumWallLength = 4f;
    public static readonly float _minimumWallHeight = 5f;
    public static readonly float _wallThickness = 1f;
    public static readonly float _wallHeight = 7f;
    public static readonly float _wallColliderThickness = 1f;

    #endregion

    #region OpeningDefaults

    public static float _doorWidth = 2.5f;
    public static float _doorHeight = 4f;

    public static float _windowWidth = 2f;
    public static float _windowHeight = 2f;

    #endregion

    #region Variables_PointsManagements

    public static float PointSnapThreshold
    {
        get
        {
            Camera cam = Camera.main;
            if (cam == null) return 2f;

            if (cam.orthographic)
            {
                OrthoCam OrthoCamRef = cam.GetComponent<OrthoCam>();
                // OrthoCam: interpolate threshold between 2 (zoom in) and 4 (zoom out)
                float t = Mathf.InverseLerp(
                    OrthoCamRef.GetMinOrthoSize(),
                    OrthoCamRef.GetMaxOrthoSize(),
                    cam.orthographicSize
                );
                //Debug.Log($"Current Snap Value {Mathf.Lerp(2f, 4f, t)}");
                return Mathf.Lerp(2f, 4f, t);
            }
            else
            {
                // PerspCam: interpolate based on distance
                PerspCam persp = cam.GetComponent<PerspCam>();
                if (persp != null)
                {
                    float t = Mathf.InverseLerp(2f, 50f, persp.GetCurrentDistance());
                    return Mathf.Lerp(2f, 4f, t);
                }
            }

            return 2f;
        }
    }

    public static float NearestWallSnapThreshold => PointSnapThreshold;

    #endregion

    public static Material _defaultFloorMaterial = Resources.Load<Material>("ProceduralMaterials/DefaultFloorMaterial");


    public static readonly float _lrYPos = 0.1f;
    public static readonly float _lrThickness = 0.5f;

    // this check if distance between two point is 
    public static bool CanSnapPoint(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < PointSnapThreshold;
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
        if (Mathf.Abs(startPosition.x - endPosition.x) < PointSnapThreshold)
        {
            endPosition = new Vector3(startPosition.x, endPosition.y, endPosition.z);
        }
        else if (Mathf.Abs(startPosition.z - endPosition.z) < PointSnapThreshold)
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

    public static Vector3 CalculateCentroid(List<Vector3> vertices)
    {
        if (vertices == null || vertices.Count < 3)
        {
            return Vector3.zero;
        }

        Vector3 centroid = Vector3.zero;
        float signedArea = 0.0f;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 p0 = vertices[i];
            Vector3 p1 = vertices[(i + 1) % vertices.Count]; // Wrap around to the first vertex

            // Using the XZ plane for 2D calculations
            float areaSegment = (p0.x * p1.z) - (p1.x * p0.z);
            signedArea += areaSegment;

            centroid.x += (p0.x + p1.x) * areaSegment;
            centroid.z += (p0.z + p1.z) * areaSegment;
        }

        if (Mathf.Approximately(signedArea, 0))
        {
            // Fallback for collinear points: return the average position
            Vector3 average = Vector3.zero;
            foreach (var v in vertices) { average += v; }
            return average / vertices.Count;
        }

        signedArea *= 0.5f;
        centroid.x /= (6.0f * signedArea);
        centroid.z /= (6.0f * signedArea);

        // Preserve the average Y position if it's relevant
        float avgY = 0;
        foreach (var v in vertices) { avgY += v.y; }
        centroid.y = avgY / vertices.Count;


        return centroid;
    }

    

    public static bool IsClockwise(Vector3 pointA, Vector3 pointB)
    {
        return (pointA.x * pointB.z - pointA.z * pointB.x) < 0;
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

    public static bool IsPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        Vector2 p2 = new Vector2(p.x, p.z);

        Vector2 ab = b2 - a2;
        Vector2 ap = p2 - a2;

        // Degenerate case: a and b are the same point
        if (ab == Vector2.zero)
            return p2 == a2;

        // Collinearity check in 2D: cross product is a scalar
        if (Mathf.Abs(ab.x * ap.y - ab.y * ap.x) > Mathf.Epsilon)
            return false;

        // Check if projection is within the segment
        float dot = Vector2.Dot(ap, ab);
        if (dot < 0 || dot > ab.sqrMagnitude)
            return false;

        return true;
    }

    public static float GetXZDistanceBetweenTwoVector(Vector3 a, Vector3 b)
    {
        Vector2 aa = new Vector2(a.x, a.z);
        Vector2 bb = new Vector2(b.x, b.z);

        return Vector2.Distance(aa, bb);
    }

    public static bool TrySnapToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd, out Vector3 snappedPoint)
    {
        snappedPoint = point;

        if (Vector3.Distance(point, lineEnd) <= 1 || Vector3.Distance(point, lineStart) <= 1)
        {
            Debug.Log("Point is Too Close to the end");
            return false;
        }

        if (lineStart == lineEnd)
            return false;

        Vector3 lineDir = lineEnd - lineStart;
        float t = Vector3.Dot(point - lineStart, lineDir) / lineDir.sqrMagnitude;
        t = Mathf.Clamp01(t);

        Vector3 closest = lineStart + t * lineDir;
        float dist = Vector3.Distance(point, closest);

        if (dist <= NearestWallSnapThreshold)
        {
            snappedPoint = closest;
            return true;
        }

        return false;
    }

    public static bool CanPlaceOpening<T>(Wall wall, Vector3 position, Opening currentOpening = null) where T : Opening
    {
        if (wall == null) return false;

        float openingWidth = GetDefaultWidth<T>();

        // --- Check against wall ends ---
        if (GetXZDistance(position, wall.GetStartPosition()) < openingWidth + 0.25f ||
            GetXZDistance(position, wall.GetEndPosition()) < openingWidth + 0.25f)
            return false;

        // --- Check against existing openings on the wall ---
        foreach (Opening existing in wall._allOpenings)
        {
            if (existing == null || (currentOpening != null && existing == currentOpening))
            {
                Debug.Log("Existing Door Found or Door is Null");
                continue;
            }
            // Skip if it's the same opening (during edit)
            if (existing is T typed && typed.OpeningPosition == position)
                continue;

            float minSpacing = (existing.Width / 2f) + (openingWidth / 2f) + 0.25f;
            if (GetXZDistance(position, existing.transform.position) < minSpacing)
                return false;
        }

        return true;
    }

    private static float GetDefaultWidth<T>() where T : Opening
    {
        if (typeof(T) == typeof(Door)) return _doorWidth;
        if (typeof(T) == typeof(Window)) return _windowWidth;

        return 2f;
    }

    public static float GetXZDistance(Vector3 a, Vector3 b)
    {
        Vector2 aXZ = new Vector2(a.x, a.z);
        Vector2 bXZ = new Vector2(b.x, b.z);
        return Vector2.Distance(aXZ, bXZ);
    }


    #region Helper for Drawing 


    public static void AddAdditionalWallPoint(WallPoint wallPoint, Wall wall = null)
    {
        if (wall != null)
        {
            wallPoint.AddConnectedWallPoint(wall.GetStartWallPoint());
            wallPoint.AddConnectedWallPoint(wall.GetEndWallPoint());
        }
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

        if (closestXDiff < PointSnapThreshold)
        {
            currentPosition.x = snapX.Value;
        }

        if (closestZDiff < PointSnapThreshold)
        {
            currentPosition.z = snapZ.Value;
        }

        return currentPosition;
    }

    public static void AddCurrentWallpoint(Wall wall, WallPoint currentWallpoint)
    {
        if (wall == null || currentWallpoint == null)
            return;
        wall.GetStartWallPoint().AddConnectedWallPoint(currentWallpoint);
        wall.GetEndWallPoint().AddConnectedWallPoint(currentWallpoint);
    }

    public static void SplitConnectedWall(Wall wall, WallPoint splitPoint, Transform strandedWall)
    {
        if (wall == null)
            return;

        DrawWall(wall.GetStartWallPoint(), splitPoint, strandedWall);
        DrawWall(splitPoint, wall.GetEndWallPoint(), strandedWall);

        wall.GetStartWallPoint().RemoveConnectedWallPoint(wall.GetEndWallPoint());
        wall.GetEndWallPoint().RemoveConnectedWallPoint(wall.GetStartWallPoint());
        WallManager.Instance.DeleteWall(wall);
    }

    public static Wall DrawWall(WallPoint startPoint, WallPoint endPoint, Transform strandedWalls = null)
    {
        GameObject wallGO = new GameObject($"Wall_{WallManager._wallIndex++}");
        Wall wallComp = wallGO.AddComponent<Wall>();
        wallGO.transform.SetParent(strandedWalls);
        wallComp.SetStartAndEndPosition(startPoint, endPoint);

        WallManager.Instance._allWalls.Add(wallComp);

        AddWallToWallPoint(startPoint, wallComp);
        AddWallToWallPoint(endPoint, wallComp);


        return wallComp;
    }

    public static void AddWallToWallPoint(WallPoint wallpoint, Wall wall)
    {
        wallpoint.AddConnectedWall(wall);
    }

    public static void ManageWallsAndWallPoints(Vector3 start, Vector3 end, Transform _strandedWalls = null)
    {
        float endpointSnapThreshold = NearestWallSnapThreshold;

        #region Phase 1: Snapping Start Point
        Vector3 bestSnapForStart = start;
        float minStartDistSq = endpointSnapThreshold * endpointSnapThreshold;
        foreach (Wall existingWall in WallManager.Instance._allWalls)
        {
            Vector3 existingWallStart = existingWall.GetStartPosition();
            float distToStartSq = (start - existingWallStart).sqrMagnitude;
            if (distToStartSq < minStartDistSq)
            {
                minStartDistSq = distToStartSq;
                bestSnapForStart = existingWallStart;
            }
            Vector3 existingWallEnd = existingWall.GetEndPosition();
            float distToEndSq = (start - existingWallEnd).sqrMagnitude;
            if (distToEndSq < minStartDistSq)
            {
                minStartDistSq = distToEndSq;
                bestSnapForStart = existingWallEnd;
            }
        }
        start = bestSnapForStart;

        // Find and apply best snap for end
        Vector3 bestSnapForEnd = end;
        float minEndDistSq = endpointSnapThreshold * endpointSnapThreshold;
        foreach (Wall existingWall in WallManager.Instance._allWalls)
        {
            Vector3 existingWallStart = existingWall.GetStartPosition();
            float distToStartSq = (end - existingWallStart).sqrMagnitude;
            if (distToStartSq < minEndDistSq)
            {
                minEndDistSq = distToStartSq;
                bestSnapForEnd = existingWallStart;
            }
            Vector3 existingWallEnd = existingWall.GetEndPosition();
            float distToEndSq = (end - existingWallEnd).sqrMagnitude;
            if (distToEndSq < minEndDistSq)
            {
                minEndDistSq = distToEndSq;
                bestSnapForEnd = existingWallEnd;
            }
        }
        end = bestSnapForEnd;
        #endregion

        WallPoint newWallStartPoint = WallPointManager.Instance.CreateOrGetwallPoints(start);
        WallPoint newWallEndPoint = WallPointManager.Instance.CreateOrGetwallPoints(end);

        List<Wall> wallsToCreate = new List<Wall>();

        #region Detection Loop 1: Start Point T-Junction
        Wall startPointWall = null;
        foreach (Wall existingWall in WallManager.Instance._allWalls)
        {
            if (AppHelper.IsPointOnLineSegment(existingWall.GetStartPosition(), existingWall.GetEndPosition(), start) &&
                Vector3.Distance(start, existingWall.GetStartPosition()) > endpointSnapThreshold &&
                Vector3.Distance(start, existingWall.GetEndPosition()) > endpointSnapThreshold)
            {
                startPointWall = existingWall;
                break;
            }
        }
        #endregion

        #region Detection Loop 2: End Point T-Junction
        Wall endPointWall = null;
        foreach (Wall existingWall in WallManager.Instance._allWalls)
        {
            if (AppHelper.IsPointOnLineSegment(existingWall.GetStartPosition(), existingWall.GetEndPosition(), end) &&
                Vector3.Distance(end, existingWall.GetStartPosition()) > endpointSnapThreshold &&
                Vector3.Distance(end, existingWall.GetEndPosition()) > endpointSnapThreshold)
            {
                endPointWall = existingWall;
                break;
            }
        }
        #endregion

        #region Detection Loop 3: Find ALL Intersections
        var intersections = new List<WallIntersection>();
        foreach (Wall existingWall in WallManager.Instance._allWalls)
        {
            if (existingWall == startPointWall || existingWall == endPointWall)
            {
                continue;
            }
            if (AppHelper.TryGetLineIntersection(start, end, existingWall.GetStartPosition(), existingWall.GetEndPosition(), out Vector3 foundIntersection) &&
                Vector3.Distance(foundIntersection, start) > endpointSnapThreshold &&
                Vector3.Distance(foundIntersection, end) > endpointSnapThreshold &&
                Vector3.Distance(foundIntersection, existingWall.GetStartPosition()) > endpointSnapThreshold &&
                Vector3.Distance(foundIntersection, existingWall.GetEndPosition()) > endpointSnapThreshold)
            {
                intersections.Add(new WallIntersection
                {
                    Point = foundIntersection,
                    IntersectedWall = existingWall,
                    DistanceFromStartSq = (foundIntersection - start).sqrMagnitude
                });
            }
        }
        #endregion

        // Step 1: Handle modifications to EXISTING walls (T-Junctions).
        if (startPointWall != null)
        {
            Debug.Log("Handling T-Junction for start point.");
            AppHelper.AddAdditionalWallPoint(newWallStartPoint, startPointWall);
            AppHelper.AddCurrentWallpoint(startPointWall, newWallStartPoint);
            AppHelper.SplitConnectedWall(startPointWall, newWallStartPoint, _strandedWalls);
        }

        if (endPointWall != null)
        {
            Debug.Log("Handling T-Junction for end point.");
            AppHelper.AddAdditionalWallPoint(newWallEndPoint, endPointWall);
            AppHelper.AddCurrentWallpoint(endPointWall, newWallEndPoint);
            AppHelper.AddCurrentWallpoint(endPointWall, newWallEndPoint);
            AppHelper.SplitConnectedWall(endPointWall, newWallEndPoint, _strandedWalls);
        }

        // Step 2: Handle creation of the NEW wall(s).
        if (intersections.Count > 0)
        {
            // Case A: The new wall is split into multiple segments by cross-intersections.
            Debug.Log($"Found {intersections.Count} intersections. Creating segments.");
            intersections.Sort((a, b) => a.DistanceFromStartSq.CompareTo(b.DistanceFromStartSq));

            WallPoint lastPoint = newWallStartPoint;
            foreach (var intersection in intersections)
            {
                WallPoint intersectionWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(intersection.Point);
                wallsToCreate.Add(AppHelper.DrawWall(lastPoint, intersectionWallPoint, _strandedWalls));

                AppHelper.AddAdditionalWallPoint(intersectionWallPoint, intersection.IntersectedWall);
                AppHelper.AddCurrentWallpoint(intersection.IntersectedWall, intersectionWallPoint);
                AppHelper.SplitConnectedWall(intersection.IntersectedWall, intersectionWallPoint, _strandedWalls);

                lastPoint = intersectionWallPoint;
            }
            wallsToCreate.Add(AppHelper.DrawWall(lastPoint, newWallEndPoint, _strandedWalls));
        }
        else
        {
            // Case B: No cross-intersections. The new wall is a single segment.
            // This correctly creates the wall for both standalone cases and T-junction cases.
            
            //Debug.Log("No intersections. Creating a single new wall.");
            wallsToCreate.Add(AppHelper.DrawWall(newWallStartPoint, newWallEndPoint, _strandedWalls));
        }

        // Step 3: Connect the points for all newly created walls.
        foreach (Wall newWall in wallsToCreate)
        {
            newWall.GetStartWallPoint().AddConnectedWallPoint(newWall.GetEndWallPoint());
            newWall.GetEndWallPoint().AddConnectedWallPoint(newWall.GetStartWallPoint());
        }
    }

    public static bool WallExists(WallPoint a, WallPoint b)
    {
        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if ((wall.GetStartWallPoint() == a && wall.GetEndWallPoint() == b) ||
                (wall.GetStartWallPoint() == b && wall.GetEndWallPoint() == a))
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}
