using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallPointManager : MonoBehaviour
{
    public static WallPointManager Instance;

    public List<WallPoint> _allWallPoints = new List<WallPoint>();
    public static int WallPointCountIndex = 0;
    public WallPoint _currentActiveWallpoint;
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

        GameObject pointGO = new GameObject(string.IsNullOrEmpty(name) ? $"WallPoint_{WallPointCountIndex++}" : name);
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

            if (Vector3.Distance(wp._position, position) < AppHelper.PointSnapThreshold)
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
            WallManager.Instance.DeleteWall(wall);
        }

        // Remove Connected wallPoint references
        foreach (WallPoint wp in wallPoint.GetConnectedWallPoints().ToList())
        {
            wp.RemoveConnectedWallPoint(wallPoint);
        }

        _allWallPoints.Remove(wallPoint);
        WallPointCountIndex--;
        Destroy(wallPoint._activeSphere);
        Destroy(wallPoint.gameObject);

        AppEventHandler.InvokeOnWallCreation();
    }

    public void RemoveStandaloneWallpoints()
    {
        for (int i = _allWallPoints.Count - 1; i >= 0; i--)
        {
            WallPoint wp = _allWallPoints[i];
            if (wp.GetConnectedWallPoints().Count == 0)
            {
                _allWallPoints.RemoveAt(i);
                Destroy(wp.gameObject);
            }
        }
    }

    public void RemoveAllStandalonePointOnwall()
    {
        foreach(WallPoint wp in new List<WallPoint>(_allWallPoints))
        {
            RemoveStandaloneWallPointOnWall(wp);
        }
    }

    public void RemoveStandaloneWallPointOnWall(WallPoint wp)
    {
        if (wp.GetConnectedWalls().Count != 2 || wp.GetConnectedWallPoints().Count != 2)
            return;

        Debug.Log("<color=blue>deleting point to merge line</color>");
        WallPoint wp1 = wp.GetConnectedWallPoints()[0];
        WallPoint wp2 = wp.GetConnectedWallPoints()[1];

        Wall w1 = wp.GetConnectedWalls()[0];
        Wall w2 = wp.GetConnectedWalls()[1];

        if (!AppHelper.IsPointOnLineSegment(wp1._position, wp2._position, wp._position))
            return;

        DeleteWallPoint(wp);
        Transform wallParent = GameObject.Find("StrandedWalls").transform;

        WallManager.Instance.DeleteWall(w1);
        WallManager.Instance.DeleteWall(w2);

        AppHelper.ManageWallsAndWallPoints(wp1._position, wp2._position, wallParent);
    }
}
