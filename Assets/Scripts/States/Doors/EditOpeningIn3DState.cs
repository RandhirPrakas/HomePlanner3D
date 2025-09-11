using System.Collections.Generic;
using UnityEngine;

public class EditOpeningIn3DState : ICameraSubState
{
    private Opening _selectedOpening;
    private Camera _camera;
    private Plane _dragPlane;
    private Vector3 _dragOffset;

    private float _snapThreshold = 2f;

    public EditOpeningIn3DState(Camera camera)
    {
        _camera = camera ?? Camera.main;
    }

    public void Enter()
    {
        Debug.Log("Entered EditOpeningIn3DState");
    }

    public void Exit()
    {
        Debug.Log("Exited EditOpeningIn3DState");
        _selectedOpening = null;
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.collider.TryGetComponent<Opening>(out var opening))
            {
                _selectedOpening = opening;

                _dragPlane = new Plane(-_camera.transform.forward, hit.point);

                if (_dragPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    _dragOffset = _selectedOpening.transform.position - hitPoint;
                }
            }
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedOpening == null) return;

        Ray ray = _camera.ScreenPointToRay(screenPos);
        if (!_dragPlane.Raycast(ray, out float enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);
        Vector3 targetPos = hitPoint + _dragOffset;

        Wall nearestWall = FindNearestWall(targetPos, out Vector3 closestPoint);
        if (nearestWall != null)
        {
            targetPos = new Vector3(closestPoint.x, _selectedOpening.transform.position.y, closestPoint.z);

            if (_selectedOpening.ParentWall != nearestWall)
            {
                Wall oldWall = _selectedOpening.ParentWall;
                _selectedOpening._lastWall = oldWall;
                if (oldWall != null)
                    oldWall._allOpenings.Remove(_selectedOpening);

                _selectedOpening.transform.SetParent(nearestWall.transform, true);
                _selectedOpening._parentWall = nearestWall;

                if (!nearestWall._allOpenings.Contains(_selectedOpening))
                    nearestWall._allOpenings.Add(_selectedOpening);
            }

            _selectedOpening.OpeningPosition = nearestWall.transform.InverseTransformPoint(targetPos);

            Vector3 dir = (nearestWall.GetEndPosition() - nearestWall.GetStartPosition()).normalized;
            _selectedOpening.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            _selectedOpening.transform.position = targetPos;

            //Use centralized generator
            WallMeshGenerator.GenerateWallWithOpenings(nearestWall);

            if (_selectedOpening._lastWall != null)
            {
                WallMeshGenerator.GenerateWallWithOpenings(_selectedOpening._lastWall);
                _selectedOpening._lastWall = null;
            }
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedOpening != null)
        {
            Debug.Log($"Moved {_selectedOpening.name} to {_selectedOpening.OpeningPosition} on {_selectedOpening.ParentWall?.name}");
            _selectedOpening = null;
        }
    }

    public void OnPinch(float delta)
    {
        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Max(0.1f, _camera.orthographicSize - delta);
        }
        else
        {
            _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - delta, 20f, 80f);
        }
    }

    public void Init(Vector3 worldPos, Vector2 screenPos) { }

    #region Helpers
    private Wall FindNearestWall(Vector3 point, out Vector3 closestPoint)
    {
        Wall nearest = null;
        float minDist = float.MaxValue;
        closestPoint = point;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if (wall == null) continue;

            Vector3 a = wall.GetStartPosition();
            Vector3 b = wall.GetEndPosition();

            Vector3 proj = ClosestPointOnLine(a, b, point);
            float dist = Vector3.Distance(proj, point);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = wall;
                closestPoint = proj;
            }
        }

        return nearest;
    }

    private Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }
    #endregion
}
