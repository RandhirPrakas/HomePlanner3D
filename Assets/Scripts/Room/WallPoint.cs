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
