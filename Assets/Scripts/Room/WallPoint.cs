using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WallPoint : MonoBehaviour
{
    public Vector3 _position;

    public List<Wall> connectedWalls = new List<Wall>();

    public WallPoint(Vector3 position)
    { 
        GameObject go = new GameObject("WallPoint");
        go.transform.position = _position;
        _position = position;
    }

    public void SetPosition(Vector3 newPos)
    {
        _position = newPos;
        foreach (var wall in connectedWalls)
            wall.UpdateFromPoints();
    }
}
