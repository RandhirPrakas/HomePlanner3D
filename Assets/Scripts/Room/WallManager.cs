using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public static WallManager Instance;

    public List<Wall> _allWalls = new List<Wall>();

    public static int _wallIndex = 0;

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
        foreach(Room room in RoomManager.Instance._allRooms)
        {
            foreach(Wall wall in _allWalls)
            {
                if(room._roomWallPoints.Contains(wall.GetStartWallPoint()) && room._roomWallPoints.Contains(wall.GetEndWallPoint()))
                {
                    room._roomWalls.Add(wall);
                    //wall.transform.SetParent(room.transform);
                }
            }
        }

        OpeningManager.Instance.TryReattachAll();
    }

    public void DestroyWall(Wall wall)
    {
        if (wall == null) return;

        foreach (var opening in new List<Opening>(wall._allOpenings))
        {
            opening.Detach();
        }

        // Disconnect endpoints first
        wall.GetStartWallPoint()?.RemoveConnectedWall(wall);
        wall.GetEndWallPoint()?.RemoveConnectedWall(wall);

        _allWalls.Remove(wall);
        Destroy(wall.gameObject);
        _allWalls.RemoveAll(w => w == null);
    }

    public void DeleteWall(Wall wall)
    {
        if (wall == null)
            return;

        foreach (var opening in new List<Opening>(wall._allOpenings))
        {
            opening.Detach();
        }

        WallPoint start = wall.GetStartWallPoint();
        WallPoint end = wall.GetEndWallPoint();

        // Remove Connected WalPoints
        start.RemoveConnectedWallPoint(end);
        end.RemoveConnectedWallPoint(start);

        // Remove Connected Wall
        start?.RemoveConnectedWall(wall);
        end?.RemoveConnectedWall(wall);

        Destroy(wall.gameObject);
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

        //if (minDist > snapThreshold) nearest = null;
        return nearest;
    }

}
