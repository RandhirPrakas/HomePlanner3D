using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WallPoint : MonoBehaviour
{
    public Vector3 _position;
    [SerializeField] List<WallPoint> _connectedWallPoints = new List<WallPoint>();
    [SerializeField] private List<Wall> _connectedWalls = new List<Wall>();
    public GameObject _activeSphere;

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

        foreach (var neighbor in _connectedWallPoints.ToList())
        {
            if (neighbor == target)
                continue;

            neighbor._connectedWallPoints.Remove(this);

            if (!neighbor._connectedWallPoints.Contains(target))
                neighbor._connectedWallPoints.Add(target);

            if (!target._connectedWallPoints.Contains(neighbor))
                target._connectedWallPoints.Add(neighbor);
        }

        DestroyHighlightVisual();

        WallPointManager.Instance._allWallPoints.Remove(this);

        GameObject.Destroy(this.gameObject);
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
        if (!_connectedWalls.Contains(newWall))
            _connectedWalls.Add(newWall);
    }

    public void RemoveConnectedWall(Wall wall)
    {
        if (_connectedWalls.Contains(wall))
            _connectedWalls.Remove(wall);
    }
  
    public List<Wall> GetConnectedWalls()
    {
        return _connectedWalls;
    }
}
