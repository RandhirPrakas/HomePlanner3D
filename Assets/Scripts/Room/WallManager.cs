using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public static WallManager Instance;

    public List<Wall> _allWalls = new List<Wall>();

    public static int WallCountIndex = 0;

    public Wall _currentSelectedWall;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        AppEventHandler.OnRoomCreated += ManageWalls;
    }

    private void OnDisable()
    {
        AppEventHandler.OnRoomCreated -= ManageWalls;
    }

    private void ManageWalls()
    {
        foreach (Room room in RoomManager.Instance._allRooms)
        {
            foreach (Wall wall in _allWalls)
            {
                if (room._roomWallPoints.Contains(wall.GetStartWallPoint()) && room._roomWallPoints.Contains(wall.GetEndWallPoint()) && !room._roomWalls.Contains(wall))
                {
                    room._roomWalls.Add(wall);
                    //wall.transform.SetParent(room.transform);
                }
            }
        }

        OpeningManager.Instance.TryReattachAll();
    }

    public void DeleteWall(Wall wall, bool deleteOpenings = false, bool refresh = true)
    {
        if (wall == null) return;

        WallCountIndex--;
        // Clean up openings safely
        foreach (var opening in new List<Opening>(wall._allOpenings))
        {
            if (deleteOpenings)
            {

                wall._allOpenings.Remove(opening);
                OpeningManager.Instance.DeleteOpening(opening);
            }
            else
                opening.Detach(wall.GetStartPosition(), wall.GetEndPosition());
        }

        // Disconnect endpoints 
        WallPoint start = wall.GetStartWallPoint();
        WallPoint end = wall.GetEndWallPoint();

        if (start != null && end != null)
        {
            start.RemoveConnectedWallPoint(end);
            end.RemoveConnectedWallPoint(start);
        }

        start?.RemoveConnectedWall(wall);
        end?.RemoveConnectedWall(wall);

        // Remove from global wall list
        _allWalls.Remove(wall);

        // Destroy wall GameObject
        Destroy(wall.gameObject);

        // Clean out any null references
        _allWalls.RemoveAll(w => w == null);

        WallPointManager.Instance.RemoveStandaloneWallPointOnWall(start);
        WallPointManager.Instance.RemoveStandaloneWallPointOnWall(end);
        //WallPointManager.Instance.RemoveAllStandalonePointOnwall();

        // Fire event if applicable
        if(refresh)
            AppEventHandler.InvokeOnWallCreation();
    }

    public Wall FindNearestWall(Vector3 point, out Vector3 closestPoint, float snapThreshold = 5f)
    {
        Wall nearest = null;
        float minDist = float.MaxValue;
        closestPoint = point;

        foreach (Wall wall in _allWalls)
        {
            if (wall == null) continue;

            Vector3 a = wall.GetStartPosition();
            Vector3 b = wall.GetEndPosition();

            Vector3 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 1e-6f) continue;

            float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / len2);
            Vector3 proj = a + ab * t;

            float dist = Vector3.Distance(proj, point);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = wall;
                closestPoint = proj;
            }
        }

        return nearest;
    }

    public Bounds GetSceneBounds()
    {
        if (_allWalls == null || _allWalls.Count == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        // Initialize with first wall
        Bounds bounds = new Bounds(_allWalls[0].transform.position, Vector3.zero);

        foreach (Wall wall in _allWalls)
        {
            if (wall == null) continue;
            bounds.Encapsulate(wall.GetStartPosition());
            bounds.Encapsulate(wall.GetEndPosition());
        }

        return bounds;
    }

    public void CreateWallWithWallPoints(WallPoint start, WallPoint end, Transform wallParent)
    {
        foreach(Wall wall in _allWalls)
        {
            if((wall.GetStartWallPoint() == start && wall.GetEndWallPoint() == end) || (wall.GetStartWallPoint() == end && wall.GetEndWallPoint() == start))
            {
                return;
            }
        }

        AppHelper.DrawWall(start, end, wallParent);
    }

}
