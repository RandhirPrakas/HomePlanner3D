using System.Collections.Generic;
using UnityEngine;

public class EditDoorState : ICameraSubState
{
    private Door _selectedDoor;
    private GameObject _highlightParent;
    private OrthoCam _orthoCam;

    private Vector3 _doorEditStartPoint;
    private Door _lastSelectedDoor;

    public EditDoorState(OrthoCam orthoCam)
    {
        _orthoCam = (orthoCam == null) ? GameManager.Instance.GetOrthoCamera() : orthoCam;
    }

    public void Enter()
    {
        Debug.Log("Entered EditDoorState");
        _highlightParent = new GameObject("DoorHighlights");
    }

    public void Exit()
    {
        Debug.Log("Exited EditDoorState");
        if (_highlightParent != null)
            GameObject.Destroy(_highlightParent);
    }

    public void Update()
    {
        _orthoCam.Update();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _selectedDoor = GetDoorUnderTouch(worldPos);
        if (_selectedDoor != null)
        {
            _doorEditStartPoint = _selectedDoor.OpeningPosition;
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedDoor == null) return;

        PlaceDoor(worldPos);
    }


    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedDoor == null) return;

        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);
        closestPoint.y = 3f;

        if (!CanPlaceDoor(nearestWall, closestPoint))
        {
            PlaceDoor(_doorEditStartPoint);
            Debug.Log($"Door moved to {_selectedDoor.OpeningPosition} on {_selectedDoor.ParentWall.name}");
        }
        Debug.Log($"Door moved to {_selectedDoor.OpeningPosition} on {_selectedDoor.ParentWall.name}");

        _selectedDoor = null;
    }

    private Door GetDoorUnderTouch(Vector3 position)
    {
        float minDist = float.MaxValue;
        Door nearest = null;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            foreach (Opening opening in wall._allOpenings)
            {
                if (opening is Door door)
                {
                    float dist = Vector3.Distance(position, door.transform.position);
                    if (dist < minDist && dist < 5f)
                    {
                        minDist = dist;
                        nearest = door;
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
        //throw new System.NotImplementedException();
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

            Vector3 proj;
            GetClosestPointOnLine(a, b, point, out proj);

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

    private void SetDoorVisualRotation(Door door, Wall wall)
    {
        // Wall direction (along the wall)
        Vector3 a = wall.GetStartPosition();
        Vector3 b = wall.GetEndPosition();
        Vector3 wallDir = (b - a).normalized;

        Vector3 perp = Vector3.Cross(wallDir, Vector3.up).normalized;

        Quaternion targetRot = Quaternion.LookRotation(perp, Vector3.up);

        Quaternion fixRot = Quaternion.Euler(90f, 0f, 0f);
        door.OpeningVisual.transform.rotation = targetRot * fixRot;
    }

    private void PlaceDoor(Vector3 worldPos)
    {
        // Find nearest wall
        Wall nearestWall = FindNearestWall(worldPos, out Vector3 proj);
        if (nearestWall == null) return;

        // Re-parent if wall changed
        if (_selectedDoor.ParentWall != nearestWall)
        {
            Wall oldWall = _selectedDoor.ParentWall;
            _selectedDoor._lastWall = oldWall;
            Debug.Log($"Door Moved from {oldWall?.name} to {nearestWall.name}");

            // Remove from old wall list safely
            if (oldWall != null)
                oldWall._allOpenings.Remove(_selectedDoor);

            // Re-parent door to new wall
            _selectedDoor.transform.SetParent(nearestWall.transform, worldPositionStays: true);
            _selectedDoor._parentWall = nearestWall;

            SetDoorVisualRotation(_selectedDoor, nearestWall);

            // Add to new wall list
            if (!nearestWall._allOpenings.Contains(_selectedDoor))
                nearestWall._allOpenings.Add(_selectedDoor);
        }

        // Place door along wall
        proj.y = _selectedDoor.transform.position.y;
        _selectedDoor.transform.position = proj;
        _selectedDoor.OpeningPosition = nearestWall.transform.InverseTransformPoint(proj);
    }

    private bool CanPlaceDoor(Wall wall, Vector3 currentPosition)
    {

        if (AppHelper.GetXZDistanceBetweenTwoVector(currentPosition, wall.GetStartPosition()) < AppHelper._doorWidth + 0.25f ||
                AppHelper.GetXZDistanceBetweenTwoVector(currentPosition, wall.GetEndPosition()) < AppHelper._doorWidth + 0.25f)
            return false;

        if (wall._allOpenings.Count == 0)
        {
            return true;
        }

        foreach (Opening opening in wall._allOpenings)
        {
            if (opening == _selectedDoor)
                continue;
            if ((AppHelper.GetXZDistanceBetweenTwoVector(currentPosition, opening.OpeningPosition) < (opening.Width) / 2 + (AppHelper._doorWidth / 2) + 0.25f))
                return false;
        }
        return true;
    }
}
