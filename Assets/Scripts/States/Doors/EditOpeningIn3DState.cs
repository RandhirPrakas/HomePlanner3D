using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditOpeningIn3DState : ICameraSubState
{
    private Opening _selectedOpening;
    private Camera _camera;
    private Plane _dragPlane;
    private Vector3 _dragOffset;

    private float _snapThreshold = 2f; // distance to wall before snap

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

                // plane parallel to camera facing through hit point
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

        // project strictly to nearest wall line
        Wall nearestWall = FindNearestWall(targetPos, out Vector3 closestPoint);
        Debug.Log(nearestWall.gameObject.name);
        if (nearestWall != null)
        {
            targetPos = new Vector3(closestPoint.x,_selectedOpening.transform.position.y,closestPoint.z);

            // re-parent if wall changed
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

            // update opening position in wall-local space
            if(_selectedOpening.OpeningType == OpeningType.Door)
                _selectedOpening.OpeningPosition = nearestWall.transform.InverseTransformPoint(targetPos);
            else if(_selectedOpening.OpeningType == OpeningType.Window)
            {
                _selectedOpening.OpeningPosition = nearestWall.transform.InverseTransformPoint(targetPos);
            }
                // orient along wall direction
                Vector3 dir = (nearestWall.GetEndPosition() - nearestWall.GetStartPosition()).normalized;
            _selectedOpening.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            // move in world space
            _selectedOpening.transform.position = targetPos;

            // regenerate wall live
            GenerateWall(nearestWall);

            // Re-Generate Last wall
            if(_selectedOpening._lastWall != null)
            {
                GenerateWall(_selectedOpening._lastWall);
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

    private void GenerateWall(Wall wall)
    {
        List<GameObject> allSegments = new List<GameObject>();

        if (wall._allOpenings == null || wall._allOpenings.Count == 0)
        {
            allSegments.AddRange(
                ProceduarlwallGenerator.GenerateWallSegment(
                    wall.GetStartPosition(),
                    wall.GetEndPosition(),
                    wall.transform));
        }
        else
        {
            // same spanning logic as PerspectiveState.GenerateWalls
            Vector3 startWS = wall.GetStartPosition();
            Vector3 endWS = wall.GetEndPosition();

            Vector3 startLS = wall.transform.InverseTransformPoint(startWS);
            Vector3 endLS = wall.transform.InverseTransformPoint(endWS);
            Vector3 dirLS = (endLS - startLS).normalized;

            var spans = wall._allOpenings
                .Select(o =>
                {
                    Vector3 openingLS = wall.transform.InverseTransformPoint(o.OpeningPosition);

                    float along = Vector3.Dot(openingLS - startLS, dirLS);
                    float half = o.Width * 0.5f;

                    return new
                    {
                        left = along - half,
                        right = along + half,
                        centerY = openingLS.y,
                        height = o.Height,
                        type = o.OpeningType
                    };
                })
                .OrderBy(s => s.left)
                .ToList();

            Vector3 cursorLS = startLS;

            foreach (var s in spans)
            {
                Vector3 openingStartLS = startLS + dirLS * s.left;
                Vector3 openingEndLS = startLS + dirLS * s.right;

                // wall before opening
                if (Vector3.Distance(cursorLS, openingStartLS) > 0.01f)
                {
                    allSegments.AddRange(
                        ProceduarlwallGenerator.GenerateWallSegment(
                            wall.transform.TransformPoint(cursorLS),
                            wall.transform.TransformPoint(openingStartLS),
                            wall.transform));
                }

                // opening cutouts
                if (s.type == OpeningType.Door)
                {
                    allSegments.AddRange(
                        ProceduarlwallGenerator.GenerateWallSegment(
                            wall.transform.TransformPoint(openingStartLS),
                            wall.transform.TransformPoint(openingEndLS),
                            wall.transform,
                            AppHelper._wallHeight - s.height,
                            s.height));
                }
                else if (s.type == OpeningType.Window)
                {
                    float center = s.centerY;
                    float bottom = center - (s.height * 0.5f);
                    float top = center + (s.height * 0.5f);

                    if (bottom > 0.01f)
                    {
                        allSegments.AddRange(
                            ProceduarlwallGenerator.GenerateWallSegment(
                                wall.transform.TransformPoint(openingStartLS),
                                wall.transform.TransformPoint(openingEndLS),
                                wall.transform,
                                bottom,
                                0f));
                    }

                    if (AppHelper._wallHeight - top > 0.01f)
                    {
                        allSegments.AddRange(
                            ProceduarlwallGenerator.GenerateWallSegment(
                                wall.transform.TransformPoint(openingStartLS),
                                wall.transform.TransformPoint(openingEndLS),
                                wall.transform,
                                AppHelper._wallHeight - top,
                                top));
                    }
                }

                cursorLS = openingEndLS;
            }

            // after last opening
            if (Vector3.Distance(cursorLS, endLS) > 0.01f)
            {
                allSegments.AddRange(
                    ProceduarlwallGenerator.GenerateWallSegment(
                        wall.transform.TransformPoint(cursorLS),
                        wall.transform.TransformPoint(endLS),
                        wall.transform));
            }
        }

        ProceduarlwallGenerator.CombineChildMeshes(wall.transform, allSegments);
    }
    #endregion
}
