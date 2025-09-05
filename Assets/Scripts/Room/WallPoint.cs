using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WallPoint : MonoBehaviour
{
    public Vector3 _position;
    public GameObject _activeSphere;

    [SerializeField] private List<WallPoint> _connectedWallPoints = new List<WallPoint>();
    [SerializeField] private List<Wall> _connectedWalls = new List<Wall>();
    [SerializeField] private List<Room> _connectedRooms = new List<Room>();

    public List<WallPoint> GetConnectedWallPoints()
    {
        return _connectedWallPoints;
    }

    public void SetHighlightVisual(GameObject visual)
    {
        _activeSphere = visual;
    }

    public void Initialize(Vector3 position)
    {
        _position = position;
        transform.position = position;
    }

    public void SetPosition(Vector3 newPos)
    {
        _position = newPos;
        transform.position = newPos;

        if (_activeSphere != null)
            _activeSphere.transform.position = newPos;

    }

    public void MergeWith(WallPoint target)
    {
        if (target == null || target == this)
            return;

        // Redirect neighbors 
        foreach (var neighbor in _connectedWallPoints.ToList())
        {
            if (neighbor == target) continue;

            neighbor._connectedWallPoints.Remove(this);

            if (!neighbor._connectedWallPoints.Contains(target))
                neighbor._connectedWallPoints.Add(target);

            if (!target._connectedWallPoints.Contains(neighbor))
                target._connectedWallPoints.Add(neighbor);
        }

        // Redirect connected walls 
        foreach (var wall in _connectedWalls.ToList())
        {
            if (wall.GetStartWallPoint() == this)
                wall.SetStartWallPoint(target);

            if (wall.GetEndWallPoint() == this)
                wall.SetEndWallPoint(target);

            if (!target.GetConnectedWalls().Contains(wall))
                target.AddConnectedWall(wall);

            wall.UpdateFromPoints(true);
        }

        // Redirect connected rooms 
        foreach (var room in _connectedRooms.ToList())
        {
            if (room._roomWallPoints.Contains(this))
            {
                room._roomWallPoints.Remove(this);

                if (!room._roomWallPoints.Contains(target))
                    room._roomWallPoints.Add(target);
            }

            target.AddConnectedRoom(room);
            room.UpdateFloor();
        }

        DestroyHighlightVisual();
        WallPointManager.Instance._allWallPoints.Remove(this);
        Destroy(gameObject);

        AppEventHandler.InvokeOnWallCreation();
    }


    private void DestroyHighlightVisual()
    {
        if (_activeSphere != null)
        {
            GameObject.Destroy(_activeSphere);
            _activeSphere = null;
        }
    }

    public void AddConnectedWallPoint(WallPoint newConnectedWallPoint)
    {
        if(!_connectedWallPoints.Contains(newConnectedWallPoint))
            _connectedWallPoints.Add(newConnectedWallPoint);
    }

    public void RemoveConnectedWallPoint(WallPoint wallPoint)
    {
        if(_connectedWallPoints.Contains(wallPoint))
        {
            _connectedWallPoints.Remove(wallPoint);
        }
    }

    public void AddConnectedWall(Wall newWall)
    {
        if(!_connectedWalls.Contains(newWall))
        {
            _connectedWalls.Add(newWall);
        }
    }

    public void RemoveConnectedWall(Wall wall)
    {
        if(_connectedWalls.Contains(wall))
        {
            _connectedWalls.Remove(wall);
        }
    }

    public List<Wall> GetConnectedWalls()
    {
        return _connectedWalls;
    }

    public void AddConnectedRoom(Room room)
    {
        if (!_connectedRooms.Contains(room))
        {
            _connectedRooms.Add(room);
        }
    }

    public void RemoveConnectedRoom(Room room)
    {
        if (_connectedRooms.Contains(room))
            _connectedRooms.Remove(room);
    }

    public List<Room> GetConnectedRooms()
    {
        return _connectedRooms;
    }
}
