using System.Collections;
using System.Collections.Generic;
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
}
