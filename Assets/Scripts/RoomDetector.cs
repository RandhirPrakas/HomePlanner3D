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

        // 1. Use the corrected algorithm to find all minimal cycles (room boundaries).
        var cycles = FindMinimalCycles(allPoints);

        // 2. Re-introduce the filter to handle the specific case of nested rooms.
        cycles = FilterRooms(cycles);

        // Clear existing rooms before creating new ones
        foreach (Room room in RoomManager.Instance._allRooms)
        {
            if (room != null && room.gameObject != null)
                room.RemoveRoom();
        }
        RoomManager.Instance._allRooms.Clear();

        // Create new rooms from the final, filtered list of cycles
        foreach (var cycle in cycles)
        {
            if (cycle.Count > 2)
                CreateRoom(cycle);
        }

        AppEventHandler.InvokeOnRoomCreation();
    }

    private void CreateRoom(List<WallPoint> cyclePoints)
    {
        GameObject roomGO = new GameObject("Room");
        Room roomComp = roomGO.AddComponent<Room>();
        roomComp.Initialize(cyclePoints);
        RoomManager.Instance._allRooms.Add(roomComp);
        roomGO.transform.SetParent(this.transform);
    }

    /*private List<List<WallPoint>> FindMinimalCycles(List<WallPoint> allPoints)
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
                    visitedDirectedEdges.Add((previous, current));
                    currentCycle.Add(current);

                    var neighbors = adj[current];
                    if (neighbors.Count < 2) { current = null; break; }

                    Vector3 incomingVec = current._position - previous._position;
                    WallPoint bestNextPoint = null;

                    // --- CORRECTED LOGIC IS HERE ---
                    // We look for the MINIMUM angle (sharpest right turn)
                    // to find the inner, minimal rooms.
                    float minAngle = 361f;

                    foreach (var candidate in neighbors)
                    {
                        if (candidate == previous) continue;
                        Vector3 outgoingVec = candidate._position - current._position;
                        float angle = Vector3.SignedAngle(incomingVec, outgoingVec, Vector3.up);

                        // Find the smallest signed angle (the sharpest "right turn")
                        if (angle < minAngle)
                        {
                            minAngle = angle;
                            bestNextPoint = candidate;
                        }
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
    }*/

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
                    visitedDirectedEdges.Add((previous, current));
                    currentCycle.Add(current);

                    var neighbors = adj[current];
                    if (neighbors.Count < 2) { current = null; break; }

                    Vector3 incomingVec = current._position - previous._position;
                    WallPoint bestNextPoint = null;

                    // --- START OF CORRECTED AND ROBUST LOGIC ---
                    float minAngle = 361f;
                    WallPoint straightLineCandidate = null; // To hold the point that continues straight

                    foreach (var candidate in neighbors)
                    {
                        if (candidate == previous) continue;

                        Vector3 outgoingVec = candidate._position - current._position;
                        float angle = Vector3.SignedAngle(incomingVec, outgoingVec, Vector3.up);

                        // Check if this path is nearly a straight line (180 or -180 degrees)
                        if (Mathf.Abs(Mathf.Abs(angle) - 180.0f) < 1.0f)
                        {
                            straightLineCandidate = candidate;
                            continue; // Deprioritize this path for now, look for actual turns first.
                        }

                        // Find the minimum angle among the actual turns.
                        if (angle < minAngle)
                        {
                            minAngle = angle;
                            bestNextPoint = candidate;
                        }
                    }

                    // If no real turn was found (bestNextPoint is still null),
                    // then our only option is to go straight.
                    if (bestNextPoint == null)
                    {
                        bestNextPoint = straightLineCandidate;
                    }
                    // --- END OF CORRECTED AND ROBUST LOGIC ---

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
        // If we only found one room, there's nothing to filter.
        if (cycles.Count <= 1)
            return cycles;

        // 1. Calculate the bounding box for every cycle found.
        var cyclesWithBounds = cycles.Select(cycle =>
        {
            if (cycle.Count == 0)
                return new { cycle, bounds = new Bounds() }; // Handle empty cycle case

            var bounds = new Bounds(cycle[0]._position, Vector3.zero);
            foreach (var point in cycle)
            {
                bounds.Encapsulate(point._position);
            }
            return new { cycle, bounds };
        }).ToList();

        // 2. Find the cycle with the largest bounding box. This is our candidate for the "exterior" room.
        var largestBoundsEntry = cyclesWithBounds
            .OrderByDescending(x => x.bounds.size.x * x.bounds.size.z)
            .FirstOrDefault();

        if (largestBoundsEntry == null)
            return cycles; // Should not happen with valid cycles

        // 3. Verify it's the exterior by making sure it contains ALL other rooms' bounds.
        bool isExterior = true;
        foreach (var entry in cyclesWithBounds)
        {
            if (entry == largestBoundsEntry) continue;

            // A contains B if A.min <= B.min and A.max >= B.max
            if (largestBoundsEntry.bounds.min.x > entry.bounds.min.x ||
                largestBoundsEntry.bounds.min.z > entry.bounds.min.z ||
                largestBoundsEntry.bounds.max.x < entry.bounds.max.x ||
                largestBoundsEntry.bounds.max.z < entry.bounds.max.z)
            {
                // If the largest bounds fails to contain even one other bounds, it's not the exterior.
                isExterior = false;
                break;
            }
        }

        // 4. If we confirmed it's the exterior room, return a list of all OTHER rooms.
        if (isExterior)
        {
            return cycles.Where(c => c != largestBoundsEntry.cycle).ToList();
        }

        // 5. Fallback: If no single room contains all others (e.g., two separate buildings),
        // then there is no "exterior" room to remove, so return everything.
        return cycles;
    }

    /*private List<List<WallPoint>> FilterRooms(List<List<WallPoint>> allCycles)
    {
        if (allCycles.Count <= 1)
            return allCycles;

        var finalRooms = new List<List<WallPoint>>();

        // For every cycle, check if it contains any OTHER cycle.
        for (int i = 0; i < allCycles.Count; i++)
        {
            var candidateRoom = allCycles[i];
            bool isMinimal = true; // Assume the room is valid until proven otherwise.

            for (int j = 0; j < allCycles.Count; j++)
            {
                if (i == j) continue; // Don't check a room against itself.

                var otherRoom = allCycles[j];

                // If the candidate room is bigger and contains another room, it's not a minimal room.
                // We use a simple area check as a performance shortcut before the more expensive polygon check.
                if (PolygonArea(candidateRoom) > PolygonArea(otherRoom))
                {
                    // Get a guaranteed inside point of the smaller room.
                    Vector3 pointInsideOther = GetGuaranteedInsidePoint(otherRoom);

                    // Check if our candidate room contains this point.
                    if (PointInPolygon(pointInsideOther, candidateRoom))
                    {
                        isMinimal = false; 
                        break;
                    }
                }
            }

            // If, after checking against all other rooms, isMinimal is still true, it's a valid room.
            if (isMinimal)
            {
                finalRooms.Add(candidateRoom);
            }
        }

        return finalRooms;
    }*/

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

    private Vector3 GetGuaranteedInsidePoint(List<WallPoint> polygon)
    {
        if (polygon == null || polygon.Count < 3)
        {
            return Vector3.zero; // Or handle error appropriately
        }
        // The centroid of the first triangle is guaranteed to be inside the polygon
        Vector3 p1 = polygon[0]._position;
        Vector3 p2 = polygon[1]._position;
        Vector3 p3 = polygon[2]._position;

        return (p1 + p2 + p3) / 3.0f;
    }

    /*private bool PointInPolygon(Vector3 point, List<WallPoint> polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector3 pi = polygon[i]._position;
            Vector3 pj = polygon[j]._position;
            if (((pi.z > point.z) != (pj.z > point.z)) &&
                (point.x < (pj.x - pi.x) * (point.z - pi.z) / (pj.z - pi.z) + pi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }*/

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
    }

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