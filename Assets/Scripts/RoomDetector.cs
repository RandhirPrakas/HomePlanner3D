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
        AppEventHandler.OnWallCreation += (() => DetectRooms(WallPointManager.Instance._allWallPoints)); 
    }

    private void OnDisable()
    {
        AppEventHandler.OnWallCreation -= (() => DetectRooms(WallPointManager.Instance._allWallPoints));
    }

    public void DetectRooms(List<WallPoint> allPoints)
    {
        var cycles = Johnson(allPoints);
        cycles = FilterRooms(cycles);

        foreach(Room room in RoomManager.Instance._allRooms)
        {
            Destroy(room.gameObject);
        }
        RoomManager.Instance._allRooms.Clear();

        foreach (var cycle in cycles)
        {
            if(cycle.Count>2)
                CreateRoom(cycle);
        }

        // Manage walls
        // All Rooms should be Created by now
        AppEventHandler.InvokeOnRoomCreation();
    }

    private void CreateRoom(List<WallPoint> cyclePoints)
    {
        GameObject roomGO = new GameObject("Room");
        Room roomComp = roomGO.AddComponent<Room>();
        roomComp.Initialize(cyclePoints);
        RoomManager.Instance._allRooms.Add(roomComp);

        // Parent for organization
        roomGO.transform.SetParent(this.transform);
    }


    private List<List<WallPoint>> Johnson(List<WallPoint> allPoints)
    {
        var index = new Dictionary<WallPoint, int>();
        for (int i = 0; i < allPoints.Count; i++)
            index[allPoints[i]] = i;

        var adj = new Dictionary<WallPoint, List<WallPoint>>();
        foreach (var p in allPoints)
            adj[p] = p.GetConnectedWallPoints();

        var cycles = new List<List<WallPoint>>();
        var blocked = new Dictionary<WallPoint, bool>();
        var B = new Dictionary<WallPoint, HashSet<WallPoint>>();
        var stack = new Stack<WallPoint>();

        void Unblock(WallPoint u)
        {
            blocked[u] = false;
            foreach (var w in B[u].ToList())
            {
                B[u].Remove(w);
                if (blocked[w])
                    Unblock(w);
            }
        }

        bool Circuit(WallPoint v, WallPoint start)
        {
            bool f = false;
            stack.Push(v);
            blocked[v] = true;

            foreach (var w in adj[v])
            {
                if (w == start)
                {
                    // Found a cycle
                    cycles.Add(stack.Reverse().ToList());
                    f = true;
                }
                else if (!blocked[w])
                {
                    if (Circuit(w, start))
                        f = true;
                }
            }

            if (f)
            {
                Unblock(v);
            }
            else
            {
                foreach (var w in adj[v])
                {
                    if (!B[w].Contains(v))
                        B[w].Add(v);
                }
            }

            stack.Pop();
            return f;
        }

        // Main loop
        foreach (var s in allPoints)
        {
            foreach (var p in allPoints)
            {
                blocked[p] = false;
                B[p] = new HashSet<WallPoint>();
            }

            Circuit(s, s);
        }

        // Deduplicate cycles (rooms may appear multiple times rotated/flipped)
        var distinctCycles = cycles.Distinct(new CycleComparer()).ToList();

        return distinctCycles;
    }

    // Shoelace formula for polygon area (XZ-plane since y is height)
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

    // Point-in-polygon (ray casting)
    private bool PointInPolygon(Vector3 point, List<WallPoint> polygon)
    {
        bool inside = false;
        int j = polygon.Count - 1;
        for (int i = 0; i < polygon.Count; j = i++)
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
    }

    // Filter cycles -> keep only minimal rooms
    private List<List<WallPoint>> FilterRooms(List<List<WallPoint>> cycles)
    {
        // Remove trivial cycles
        var valid = cycles.Where(c => c.Count >= 3).ToList();

        // Sort by area (smallest first)
        var withArea = valid
            .Select(c => new { cycle = c, area = PolygonArea(c) })
            .OrderBy(a => a.area)
            .ToList();

        var finalRooms = new List<List<WallPoint>>();

        foreach (var entry in withArea)
        {
            bool containsSmaller = false;

            // Check if this polygon fully contains any already accepted smaller polygon
            foreach (var existingRoom in finalRooms)
            {
                // *** THIS IS THE ONLY PART THAT CHANGES ***
                // OLD check was: if (existingRoom.All(p => PointInPolygon(p._position, entry.cycle)))
                // NEW check uses the centroid for a more robust test.
                Vector3 centroidOfExisting = GetPolygonCentroid(existingRoom);
                if (PointInPolygon(centroidOfExisting, entry.cycle))
                {
                    // This large cycle wraps around a smaller one -> discard it
                    containsSmaller = true;
                    break;
                }
            }

            if (!containsSmaller)
                finalRooms.Add(entry.cycle);
        }

        return finalRooms;
    }
    // Calculates the centroid (geometric center) of a polygon on the XZ plane
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
            return Vector3.zero; // Or handle this case as you see fit

        accumulatedArea *= 3f;
        return new Vector3(centerX / accumulatedArea, 0, centerZ / accumulatedArea);
    }

    // Helper: expose connected points
    // (add this to WallPoint if you want, but here I’ll just assume accessor)
    // public List<WallPoint> GetConnectedPoints() => _connectedWallPoints;

    private class CycleComparer : IEqualityComparer<List<WallPoint>>
    {
        public bool Equals(List<WallPoint> a, List<WallPoint> b)
        {
            if (a.Count != b.Count) return false;
            return new HashSet<WallPoint>(a).SetEquals(b);
        }

        public int GetHashCode(List<WallPoint> obj)
        {
            int hash = 17;
            foreach (var p in obj)
                hash ^= p.GetHashCode();
            return hash;
        }
    }
}
