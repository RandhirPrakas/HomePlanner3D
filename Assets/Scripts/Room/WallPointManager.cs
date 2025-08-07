using System.Collections.Generic;
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

        GameObject pointGO = new GameObject(string.IsNullOrEmpty(name) ? "WallPoint" : name);
        WallPoint wallPoint = pointGO.AddComponent<WallPoint>();
        wallPoint.Initialize(position);
        _allWallPoints.Add(wallPoint);

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


}
