using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallPointManager : MonoBehaviour
{
    public static WallPointManager Instance;

    public List<WallPoint> _allWallPoints = new List<WallPoint>();

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

    public WallPoint CreateOrGetwallPoints(Vector3 position, string name = null)
    {

        foreach(WallPoint wallpoint in _allWallPoints)
        {
            if(AppHelper.CanSnapPoint(position, wallpoint._position))
                return wallpoint;
            
        }

        GameObject pointGO = new GameObject(string.IsNullOrEmpty(name) ? $"WallPoint_{RoomManager.WallPointCountIndex++}" : name);
        WallPoint wallPoint = pointGO.AddComponent<WallPoint>();
        wallPoint.Initialize(position);
        _allWallPoints.Add(wallPoint);
        wallPoint.transform.SetParent(this.transform);

        _allWallPoints = SortClockwiseFromOrigin();

        return wallPoint;
    }

    public WallPoint GetExistingPointAt(Vector3 position, WallPoint wallpoint = null)
    {
        foreach (var wp in _allWallPoints)
        {
            if (wp == wallpoint) continue;

            if (Vector3.Distance(wp._position, position) < AppHelper._pointSnapThreshold)
            {
                return wp;
            }
        }

        return null;
    }

  

    public List<WallPoint> SortClockwiseFromOrigin()
    {
        if (_allWallPoints == null || _allWallPoints.Count == 0)
            return _allWallPoints;

        return _allWallPoints
            .OrderByDescending(p =>
            {
                // Compute angle in radians
                float angle = Mathf.Atan2(p._position.z, p._position.x);

                // Normalize to [0, 2π)
                if (angle < 0) angle += 2 * Mathf.PI;

                return angle;
            })
            .ToList();
    }

    public WallPoint FindNearestWallPoint(Vector3 position, float maxDistance = 0.2f)
    {
        WallPoint nearest = null;
        float minDist = float.MaxValue;

        foreach (var wp in _allWallPoints)
        {
            float dist = Vector3.Distance(position, wp._position);
            if (dist < minDist && dist <= maxDistance)
            {
                minDist = dist;
                nearest = wp;
            }
        }

        return nearest;
    }

    public void DeleteWallPoint(WallPoint wallPoint)
    {
        if (wallPoint == null)
        {
            Debug.Log("<color=red>wallPoint is Null</color>");
            return;
        }

        // Remove Room Reference
        foreach (Room room in wallPoint.GetConnectedRooms().ToList())
        {
            room._roomWallPoints.Remove(wallPoint);
        }

        // Remove Wall Reference
        foreach (Wall wall in wallPoint.GetConnectedWalls().ToList())
        {
            WallManager.Instance.DestroyWall(wall);
        }

        // Remove Connected wallPoint references
        foreach (WallPoint wp in wallPoint.GetConnectedWallPoints().ToList())
        {
            wp.RemoveConnectedWallPoint(wallPoint);
        }

        _allWallPoints.Remove(wallPoint);
        Destroy(wallPoint._activeSphere);
        Destroy(wallPoint.gameObject);

        AppEventHandler.InvokeOnWallCreation();
    }

}
