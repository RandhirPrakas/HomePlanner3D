using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<WallPoint> _roomWallPoints = new List<WallPoint>();
    public List<Wall> _roomWalls = new List<Wall>();

    public void Initialize(List<WallPoint> points)
    {
        _roomWallPoints = points;

        Debug.Log($"Room created with {_roomWallPoints.Count} points");
    }
}
