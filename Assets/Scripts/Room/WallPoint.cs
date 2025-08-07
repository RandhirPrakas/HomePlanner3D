using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WallPoint : MonoBehaviour
{
    public Vector3 _position;

    public HashSet<Wall> _connectedWalls = new HashSet<Wall>();

    public GameObject _activeSphere;

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

        foreach (var wall in _connectedWalls)
            wall.UpdateFromPoints();
    }

    public HashSet<Room> GetParentRooms()
    {
        HashSet<Room> rooms = new HashSet<Room>();
        foreach (var wall in _connectedWalls)
        {
            Room r = wall.GetCurrentRoom();
            if (r != null)
                rooms.Add(r);
        }
        return rooms;
    }



    /*public void MergeWith(WallPoint targetPoint)
    {
        if (targetPoint == this) return;

        HashSet<Room> affectedRooms = new HashSet<Room>();
        foreach (var wall in _connectedWalls)
        {
            if (wall != null && wall.GetCurrentRoom() != null)
            {
                affectedRooms.Add(wall.GetCurrentRoom());
            }
        }

        List<Wall> wallsToTransfer = new List<Wall>(_connectedWalls);

        foreach (Wall wall in wallsToTransfer)
        {
            WallPoint otherPoint = (wall.StartWallPoint == this) ? wall.EndWallPoint : wall.StartWallPoint;

            if (otherPoint == targetPoint)
            {
                targetPoint._connectedWalls.Remove(wall);
                otherPoint._connectedWalls.Remove(wall);

                GameObject.Destroy(wall.gameObject);
                continue;
            }

            if (wall.StartWallPoint == this)
                wall.StartWallPoint = targetPoint;
            else if (wall.EndWallPoint == this)
                wall.EndWallPoint = targetPoint;

            targetPoint._connectedWalls.Add(wall);


            wall.UpdateFromPoints();
        }

        if (_activeSphere != null)
            GameObject.Destroy(_activeSphere);

        WallPointManager.Instance._allWallPoints.Remove(this);
        _connectedWalls.Clear();

        GameObject.Destroy(gameObject);

    }*/

    /*public void MergeWith(WallPoint targetPoint)
    {
        if (targetPoint == this) return;

        // Collect all rooms affected before we start merging
        HashSet<Room> affectedRooms = GetParentRooms();

        List<Wall> wallsToTransfer = new List<Wall>(_connectedWalls);

        foreach (Wall wall in wallsToTransfer)
        {
            if (wall == null) continue;

            WallPoint otherPoint = (wall.StartWallPoint == this) ? wall.EndWallPoint : wall.StartWallPoint;

            // If wall would connect the same point to itself, destroy it
            if (otherPoint == targetPoint)
            {
                targetPoint._connectedWalls.Remove(wall);
                otherPoint._connectedWalls.Remove(wall);

                foreach (Room r in affectedRooms)
                    r._allRoomWalls.Remove(wall);

                Destroy(wall.gameObject);

                foreach (Room room in affectedRooms)
                {
                    room._allRoomWalls.RemoveAll(w => w == null); 
                    room.CleanUpNullWalls();
                    room.UpdateFloorOnEditingPoints();
                }
                continue;
            }

            // Reassign wall point
            if (wall.StartWallPoint == this)
                wall.StartWallPoint = targetPoint;
            else if (wall.EndWallPoint == this)
                wall.EndWallPoint = targetPoint;

            // Ensure both ends know about the wall
            if (!targetPoint._connectedWalls.Contains(wall))
                targetPoint._connectedWalls.Add(wall);
            if (!otherPoint._connectedWalls.Contains(wall))
                otherPoint._connectedWalls.Add(wall);

            wall.UpdateFromPoints(); // Visually update the wall
        }

        // Clean up this point
        if (_activeSphere != null)
            GameObject.Destroy(_activeSphere);

        WallPointManager.Instance._allWallPoints.Remove(this);
        _connectedWalls.Clear();

        GameObject.Destroy(gameObject);

        // Optional: Clean room state (null walls etc.)
        foreach (Room room in affectedRooms)
        {
            room.CleanUpNullWalls();          // Remove any null wall refs
            room.UpdateFloorOnEditingPoints(); // Refresh mesh/visuals
        }
    }*/

    public void MergeWith(WallPoint target)
    {
        if (target == null || target == this)
            return;

        foreach (Wall wall in _connectedWalls.ToList())  
        {
            if (wall.GetStartWallPoint() == this)
            {
                wall.SetStartWallPoint(target);
            }
            else if (wall.GetEndWallPoint() == this)
            {
                wall.SetEndWallPoint(target);
            }

            if (wall.GetStartWallPoint() == wall.GetEndWallPoint())
            {
                DestroyWall(wall);
                continue;
            }

            wall.UpdateFromPoints();
            target.AddConnectedWall(wall);
        }

        DestroyHighlightVisual();

        // Remove this point from the manager
        WallPointManager.Instance._allWallPoints.Remove(this);
        GameObject.Destroy(this.gameObject);
    }

    private void AddConnectedWall(Wall wall)
    {
        if (!_connectedWalls.Contains(wall))
        {
            _connectedWalls.Add(wall);
        }
    }

    private void DestroyHighlightVisual()
    {
        if (_activeSphere != null)
        {
            GameObject.Destroy(_activeSphere);
            _activeSphere = null;
        }
    }

    public void DestroyWall(Wall wall)
    {
        wall.DestroyLabel();
        GameObject.Destroy(wall.gameObject);
    }
}
