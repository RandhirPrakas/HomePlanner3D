using UnityEngine;

public class EditOpeningState<T> : ICameraSubState where T : Opening
{
    private T _selectedOpening;
    private OrthoCam _orthoCam;
    private Vector3 _startPosition;

    public EditOpeningState(OrthoCam orthoCam)
    {
        _orthoCam = orthoCam ?? GameManager.Instance.GetOrthoCamera();
    }

    public void Enter()
    {
        Debug.Log($"Entered EditOpeningState<{typeof(T).Name}>");
    }

    public void Exit()
    {
        Debug.Log($"Exited EditOpeningState<{typeof(T).Name}>");
    }

    public void Update() => _orthoCam.Update();

    public void Init(Vector3 worldPos, Vector2 screenPos) { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _selectedOpening = GetOpeningUnderTouch(worldPos);
        if (_selectedOpening != null)
            _startPosition = _selectedOpening.OpeningPosition;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedOpening != null)
            MoveOpening(_selectedOpening, worldPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedOpening == null) return;

        Wall nearest = FindNearestWall(worldPos, out Vector3 proj);
        proj.y = 3f;

        if (!AppHelper.CanPlaceOpening<T>(nearest, proj))
        {
            Debug.Log($"{typeof(T).Name} placement invalid, reverting.");
            MoveOpening(_selectedOpening, _startPosition);
        }

        _selectedOpening = null;
    }

    private T GetOpeningUnderTouch(Vector3 pos)
    {
        float minDist = float.MaxValue;
        T nearest = null;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            foreach (Opening opening in wall._allOpenings)
            {
                if (opening is T typedOpening)
                {
                    float dist = Vector3.Distance(pos, typedOpening.transform.position);
                    if (dist < minDist && dist < 5f)
                    {
                        minDist = dist;
                        nearest = typedOpening;
                    }
                }
            }
        }
        return nearest;
    }

    private void MoveOpening(T opening, Vector3 worldPos)
    {
        Wall nearest = FindNearestWall(worldPos, out Vector3 proj);
        if (nearest == null) return;

        // re-parent if wall changed
        if (opening.ParentWall != nearest)
        {
            Wall oldWall = opening.ParentWall;
            if (oldWall != null) oldWall._allOpenings.Remove(opening);

            opening.transform.SetParent(nearest.transform, true);
            opening._parentWall = nearest;

            if (!nearest._allOpenings.Contains(opening))
                nearest._allOpenings.Add(opening);

            SetOpeningRotation(opening, nearest);
        }

        proj.y = opening.transform.position.y;
        opening.transform.position = proj;
        opening.OpeningPosition = nearest.transform.InverseTransformPoint(proj);
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
            GetClosestPoint(a, b, point, out Vector3 proj);

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

    private void GetClosestPoint(Vector3 a, Vector3 b, Vector3 point, out Vector3 closest)
    {
        Vector3 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) { closest = a; return; }
        float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / len2);
        closest = a + ab * t;
    }

    private void SetOpeningRotation(T opening, Wall wall)
    {
        Vector3 dir = (wall.GetEndPosition() - wall.GetStartPosition()).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
        opening.OpeningVisual.transform.rotation = Quaternion.LookRotation(perp, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
    }

    public void OnPinch(float delta) => _orthoCam.ZoomCamera(delta);
}
