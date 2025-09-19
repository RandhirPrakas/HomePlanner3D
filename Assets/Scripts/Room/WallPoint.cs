using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        // --- STEP 0: GATHER INFORMATION ---
        // Find the wall connecting THIS point and the TARGET point.
        Wall wallToDelete = null;
        foreach (var wall in _connectedWalls)
        {
            if ((wall.StartWallPoint == this && wall.EndWallPoint == target) ||
                (wall.StartWallPoint == target && wall.EndWallPoint == this))
            {
                wallToDelete = wall;
                break;
            }
        }

        // Create lists of items that will need a full update at the end.
        var wallsToUpdate = new HashSet<Wall>();
        var roomsToUpdate = new HashSet<Room>();

        // Redirect all connected neighbors.
        foreach (var neighbor in _connectedWallPoints.ToList())
        {
            if (neighbor == target) continue; // Skip the target itself for now.

            neighbor.RemoveConnectedWallPoint(this);
            if (!neighbor.GetConnectedWallPoints().Contains(target))
                neighbor.AddConnectedWallPoint(target);

            if (!target.GetConnectedWallPoints().Contains(neighbor))
                target.AddConnectedWallPoint(neighbor);
        }

        // Redirect all connected walls.
        foreach (var wall in _connectedWalls.ToList())
        {
            if (wall == wallToDelete) continue;

            // Re-link the wall's endpoint.
            if (wall.StartWallPoint == this) wall.SetStartWallPoint(target);
            if (wall.EndWallPoint == this) wall.SetEndWallPoint(target);

            // Add the wall to the target's list.
            target.AddConnectedWall(wall);

            // Add to our list for updating LATER.
            wallsToUpdate.Add(wall);
        }

        // Redirect all connected rooms.
        foreach (var room in _connectedRooms.ToList())
        {
            room._roomWallPoints.Remove(this);
            if (!room._roomWallPoints.Contains(target))
                room._roomWallPoints.Add(target);

            target.AddConnectedRoom(room);

            // Add to our list for updating LATER.
            roomsToUpdate.Add(room);
        }

        // safely delete the redundant wall.
        if (wallToDelete != null)
        {
            WallManager.Instance.DeleteWall(wallToDelete);
        }

        foreach (var wall in wallsToUpdate)
        {
            wall.UpdateFromPoints(true);
        }
        foreach (var room in roomsToUpdate)
        {
            room.UpdateFloor();
        }

        //destroy this obsolete WallPoint.
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
