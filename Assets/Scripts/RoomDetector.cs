using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomDetector : MonoBehaviour
{
    public static RoomDetector Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        AppEventHandler.OnWallCreation += HandleWallCreation;
    }

    private void OnDisable()
    {
        AppEventHandler.OnWallCreation -= HandleWallCreation;
    }

    private void HandleWallCreation()
    {
        DetectRooms(WallPointManager.Instance._allWallPoints);
    }

    public void DetectRooms(List<WallPoint> allPoints)
    {
        foreach (var wall in WallManager.Instance._allWalls)
        {
            wall.ClearParentRooms();
        }

        var cycles = FindMinimalCycles(allPoints);

        cycles = FilterRooms(cycles);

        foreach (Room room in RoomManager.Instance._allRooms)
        {
            if (room != null && room.gameObject != null)
                room.RemoveRoom();
        }
        RoomManager.Instance._allRooms.Clear();

        foreach (var cycle in cycles)
        {
            if (cycle.Count > 2)
                CreateRoom(cycle);
        }
    }

    private void CreateRoom(List<WallPoint> cyclePoints)
    {
        GameObject roomGO = new GameObject("Room");
        Room roomComp = roomGO.AddComponent<Room>();
        roomComp.Initialize(cyclePoints);
        RoomManager.Instance._allRooms.Add(roomComp);
        roomGO.transform.SetParent(this.transform);
    }

    private List<List<WallPoint>> FindMinimalCycles(List<WallPoint> allPoints)
    {
        var cycles = new List<List<WallPoint>>();
        var visitedDirectedEdges = new HashSet<(WallPoint, WallPoint)>();
        var adj = allPoints.ToDictionary(p => p, p => p.GetConnectedWallPoints());

        foreach (var startPoint in allPoints)
        {
            foreach (var firstHop in adj[startPoint])
            {
                if (visitedDirectedEdges.Contains((startPoint, firstHop)))
                    continue;

                var currentCycle = new List<WallPoint> { startPoint };
                var previous = startPoint;
                var current = firstHop;

                while (current != startPoint)
                {
                    if (currentCycle.Contains(current))
                    {
                        current = null;
                        break;
                    }

                    visitedDirectedEdges.Add((previous, current));
                    currentCycle.Add(current);

                    var neighbors = adj[current];
                    if (neighbors.Count < 2) { current = null; break; }

                    Vector3 incomingVec = current._position - previous._position;
                    WallPoint bestNextPoint = null;

                    float minAngle = 361f;
                    WallPoint straightLineCandidate = null;

                    foreach (var candidate in neighbors)
                    {
                        if (candidate == previous) continue;

                        Vector3 outgoingVec = candidate._position - current._position;
                        float angle = Vector3.SignedAngle(incomingVec, outgoingVec, Vector3.up);

                        if (Mathf.Abs(Mathf.Abs(angle) - 180.0f) < 1.0f)
                        {
                            straightLineCandidate = candidate;
                            continue;
                        }

                        if (angle < minAngle)
                        {
                            minAngle = angle;
                            bestNextPoint = candidate;
                        }
                    }

                    if (bestNextPoint == null)
                    {
                        bestNextPoint = straightLineCandidate;
                    }

                    previous = current;
                    current = bestNextPoint;
                    if (current == null) break;
                }

                if (current == startPoint && currentCycle.Count > 2)
                {
                    visitedDirectedEdges.Add((previous, current));
                    cycles.Add(currentCycle);
                }
            }
        }
        return cycles.Distinct(new CycleComparer()).ToList();
    }

    private List<List<WallPoint>> FilterRooms(List<List<WallPoint>> cycles)
    {
        if (cycles.Count <= 1)
            return cycles;

        var cyclesWithBounds = cycles.Select(cycle =>
        {
            if (cycle.Count == 0)
                return new { cycle, bounds = new Bounds() };

            var bounds = new Bounds(cycle[0]._position, Vector3.zero);
            foreach (var point in cycle)
            {
                bounds.Encapsulate(point._position);
            }
            return new { cycle, bounds };
        }).ToList();

        var largestBoundsEntry = cyclesWithBounds
            .OrderByDescending(x => x.bounds.size.x * x.bounds.size.z)
            .FirstOrDefault();

        if (largestBoundsEntry == null)
            return cycles;

        bool isExterior = true;
        foreach (var entry in cyclesWithBounds)
        {
            if (entry == largestBoundsEntry) continue;

            if (largestBoundsEntry.bounds.min.x > entry.bounds.min.x ||
                largestBoundsEntry.bounds.min.z > entry.bounds.min.z ||
                largestBoundsEntry.bounds.max.x < entry.bounds.max.x ||
                largestBoundsEntry.bounds.max.z < entry.bounds.max.z)
            {
                isExterior = false;
                break;
            }
        }

        if (isExterior)
        {
            return cycles.Where(c => c != largestBoundsEntry.cycle).ToList();
        }

        return cycles;
    }
/*
    private float PolygonArea(List<WallPoint> points)
    {
        float area = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p1 = points[i]._position;
            Vector3 p2 = points[(i + 1) % points.Count]._position;
            area += (p1.x * p2.z - p2.x * p1.z);
        }
        return Mathf.Abs(area) * 0.5f;
    }

    private Vector3 GetPolygonCentroid(List<WallPoint> points)
    {
        float accumulatedArea = 0f;
        float centerX = 0f;
        float centerZ = 0f;

        for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
        {
            Vector3 p1 = points[i]._position;
            Vector3 p2 = points[j]._position;
            float temp = p1.x * p2.z - p2.x * p1.z;
            accumulatedArea += temp;
            centerX += (p1.x + p2.x) * temp;
            centerZ += (p1.z + p2.z) * temp;
        }

        if (Mathf.Abs(accumulatedArea) < 1e-7f)
            return Vector3.zero;

        accumulatedArea *= 3f;
        return new Vector3(centerX / accumulatedArea, 0, centerZ / accumulatedArea);
    }

    
    private bool PointInPolygon(Vector3 point, List<WallPoint> polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector3 pi = polygon[i]._position;
            Vector3 pj = polygon[j]._position;

            // Check if the point is between the y-coordinates of the edge
            if (((pi.z > point.z) != (pj.z > point.z)))
            {
                // Calculate the x-intersection of the line
                float xIntersection = (pj.x - pi.x) * (point.z - pi.z) / (pj.z - pi.z) + pi.x;

                // This check now correctly handles vertical lines because of the first condition
                if (point.x < xIntersection)
                {
                    inside = !inside;
                }
            }
        }
        return inside;
    }*/

    private class CycleComparer : IEqualityComparer<List<WallPoint>>
    {
        public bool Equals(List<WallPoint> a, List<WallPoint> b)
        {
            if (a.Count != b.Count) return false;
            return new HashSet<WallPoint>(a).SetEquals(b);
        }

        public int GetHashCode(List<WallPoint> obj)
        {
            int hash = 0;
            foreach (var p in obj.OrderBy(point => point.GetInstanceID()))
            {
                hash ^= p.GetHashCode();
            }
            return hash;
        }
    }
}