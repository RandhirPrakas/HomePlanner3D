using System.Collections.Generic;
using UnityEngine;

public class EditWindowState : ICameraSubState
{
    private Window _selectedWindow;
    private GameObject _highlightParent;

    private OrthoCam _orthoCam;

    public EditWindowState(OrthoCam orthoCam)
    {
        _orthoCam = (orthoCam == null) ? GameManager.Instance.GetOrthoCamera() : orthoCam;
    }

    public void Enter()
    {
        Debug.Log("Entered EditWindowState");
        _highlightParent = new GameObject("WindowHighlights");
        
    }

    public void Exit()
    {
        Debug.Log("Exited EditWindowState");
        if (_highlightParent != null)
            GameObject.Destroy(_highlightParent);
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _selectedWindow = GetWindowUnderTouch(worldPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedWindow == null) return;

        // Find nearest wall
        Wall nearestWall = FindNearestWall(worldPos, out Vector3 proj);
        if (nearestWall == null) return;

        // Re-parent if wall changed
        if (_selectedWindow.ParentWall != nearestWall)
        {
            Wall oldWall = _selectedWindow.ParentWall;
            Debug.Log($"Window reassigned from {oldWall?.name} to {nearestWall.name}");

            if (oldWall != null)
                oldWall._allOpenings.Remove(_selectedWindow);

            _selectedWindow.transform.SetParent(nearestWall.transform, worldPositionStays: true);
            _selectedWindow._parentWall = nearestWall;

            if (!nearestWall._allOpenings.Contains(_selectedWindow))
                nearestWall._allOpenings.Add(_selectedWindow);
        }

        // 🔹 For windows: keep their current Y (height), only snap X/Z to wall
        proj.y = _selectedWindow.transform.position.y;

        // Update position
        _selectedWindow.transform.position = proj;
        _selectedWindow.OpeningPosition = nearestWall.transform.InverseTransformPoint(proj);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedWindow == null) return;
        Debug.Log($"Window moved to {_selectedWindow.OpeningPosition} on {_selectedWindow.ParentWall.name}");
        _selectedWindow = null;
    }

    private Window GetWindowUnderTouch(Vector3 position)
    {
        float minDist = float.MaxValue;
        Window nearest = null;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            foreach (Opening opening in wall._allOpenings)
            {
                if (opening is Window window)
                {
                    float dist = Vector3.Distance(position, window.transform.position);
                    if (dist < minDist && dist < 5f) // selection threshold
                    {
                        minDist = dist;
                        nearest = window;
                    }
                }
            }
        }
        return nearest;
    }

    private void GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point, out Vector3 closest)
    {
        Vector3 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) { closest = a; return; }
        float t = Vector3.Dot(point - a, ab) / len2;
        t = Mathf.Clamp01(t);
        closest = a + ab * t;
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    private Wall FindNearestWall(Vector3 point, out Vector3 closestPoint, float snapThreshold = 5f)
    {
        Wall nearest = null;
        float minDist = float.MaxValue;
        closestPoint = point;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if (wall == null) continue;

            Vector3 a = wall.GetStartPosition();
            Vector3 b = wall.GetEndPosition();

            GetClosestPointOnLine(a, b, point, out Vector3 proj);

            float dist = Vector3.Distance(proj, point);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = wall;
                closestPoint = proj;
            }
        }

        if (minDist > snapThreshold) nearest = null;
        return nearest;
    }

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }

}
