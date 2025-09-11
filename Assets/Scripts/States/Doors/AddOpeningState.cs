using UnityEngine;

public class AddOpeningState<T> : ICameraSubState where T : Opening
{
    private Wall _targetWall;
    private GameObject _preview;
    private GameObject _prefab;
    private OrthoCam _orthoCam;

    public AddOpeningState(OrthoCam orthoCam, string prefabPath)
    {
        _prefab = Resources.Load<GameObject>(prefabPath);
        _orthoCam = orthoCam;
    }

    public void Enter()
    {
        Debug.Log($"Entered AddOpeningState<{typeof(T).Name}>");

        if (_prefab == null)
        {
            Debug.LogError($"Prefab not found for {typeof(T).Name}");
            GameManager.Instance.SetSubState(new Ortho_IdleState(_orthoCam));
            return;
        }

        _preview = GameObject.Instantiate(_prefab);
        _preview.name = $"{typeof(T).Name} Preview";

        // Place at midpoint of first wall as default
        _targetWall = FindFirstWall();
        if (_targetWall != null)
        {
            Vector3 start = _targetWall.GetStartPosition();
            Vector3 end = _targetWall.GetEndPosition();
            Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
            _preview.transform.position = new Vector3(midPoint.x, 0.5f, midPoint.z);
            SetOpeningRotation(_preview, _targetWall);
        }
    }

    public void Exit()
    {
        Debug.Log($"Exited AddOpeningState<{typeof(T).Name}>");
        if (_preview != null)
            GameObject.Destroy(_preview);
    }

    public void Update() => _orthoCam.Update();

    public void Init(Vector3 worldPos, Vector2 screenPos) { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos) => UpdatePreview(worldPos);
    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos) => UpdatePreview(worldPos);

    private void UpdatePreview(Vector3 worldPos)
    {
        if (_preview == null) return;

        Wall nearest = FindNearestWall(worldPos, out Vector3 proj);
        if (nearest != null)
        {
            _targetWall = nearest;
            _preview.transform.position = new Vector3(proj.x, 0.5f, proj.z);
            SetOpeningRotation(_preview, nearest);
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_targetWall == null) return;

        Vector3 proj;
        FindNearestWall(worldPos, out proj);
        proj.y = 3f;

        if (!AppHelper.CanPlaceOpening<T>(_targetWall, proj))
        {
            Debug.Log($"Cannot place {typeof(T).Name}, too close to another opening or wall end.");
            return;
        }

        GameObject visualGO = new GameObject(typeof(T).Name);
        visualGO.transform.position = proj;
        visualGO.transform.SetParent(_targetWall.transform);
        visualGO.tag = typeof(T).Name;

        GameObject visualizer = GameObject.Instantiate(_prefab, proj, Quaternion.identity, visualGO.transform);
        T opening = visualGO.AddComponent<T>();
        opening.Initialize(_targetWall, proj);
        opening.OpeningVisual = visualizer;

        SetOpeningRotation(opening.OpeningVisual, _targetWall);
        Debug.Log($"{typeof(T).Name} placed on {_targetWall.name} at {proj}");

        if (_preview != null)
        {
            GameObject.Destroy(_preview);
            _preview = null;
        }

        GameManager.Instance.SetSubState(new Ortho_IdleState(_orthoCam));
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
            GetClosestPoint(a, b, point, out proj);

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

    private Wall FindFirstWall()
    {
        foreach (Wall wall in WallManager.Instance._allWalls)
            if (wall != null) return wall;
        return null;
    }

    public void OnPinch(float delta) => _orthoCam.ZoomCamera(delta);

    private void SetOpeningRotation(GameObject visual, Wall wall)
    {
        Vector3 dir = (wall.GetEndPosition() - wall.GetStartPosition()).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
        visual.transform.rotation = Quaternion.LookRotation(perp, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
    }
}
