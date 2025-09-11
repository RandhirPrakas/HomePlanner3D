using UnityEngine;

public class AddWindowState : ICameraSubState
{
    private Wall _targetWall;
    private GameObject _dotPreview;
    private GameObject _dotPrefab;

    private OrthoCam _orthoCam;

    public AddWindowState(OrthoCam orthoCam)
    {
        _dotPrefab = Resources.Load<GameObject>("Prefabs/WindowDotPrefab");
        _orthoCam = orthoCam;
    }

    public void Enter()
    {
        Debug.Log("Entered AddWindowState");

        if (_targetWall == null)
        {
            _targetWall = FindFirstWall();

            if (_targetWall == null)
            {
                Debug.LogWarning("No walls exist in the scene. Exiting to IdleState.");
                GameManager.Instance.SetSubState(new Ortho_IdleState(GameManager.Instance.GetOrthoCamera()));
                return;
            }
        }

        if (_dotPrefab != null)
        {
            _dotPreview = GameObject.Instantiate(_dotPrefab);
        }
        else
        {
            _dotPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _dotPreview.name = "Window Preview";
            _dotPreview.transform.localScale = Vector3.one;
            _dotPreview.tag = "Window";
        }

        // --- Place preview at midpoint of target wall ---
        Vector3 start = _targetWall.GetStartPosition();
        Vector3 end = _targetWall.GetEndPosition();
        Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);

        float windowHeight = AppHelper._wallHeight * 0.5f; // middle of wall height
        _dotPreview.transform.position = new Vector3(midPoint.x, windowHeight, midPoint.z);

        PlaceWindow(_targetWall, midPoint);
    }

    public void Exit()
    {
        Debug.Log("Exited AddWindowState");

        if (_dotPreview != null)
        {
            GameObject.Destroy(_dotPreview);
        }
    }

    public void Update()
    {
        _orthoCam.Update();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        Debug.Log("Initialized AddWindowState");
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = AppHelper._wallHeight * 0.5f; // force window to mid-wall
        UpdatePreviewPosition(worldPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = AppHelper._wallHeight * 0.5f;
        UpdatePreviewPosition(worldPos);
    }

    private void UpdatePreviewPosition(Vector3 worldPos)
    {
        if (_dotPreview == null) return;

        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);

        if (nearestWall != null)
        {
            _targetWall = nearestWall;
            float windowHeight = AppHelper._wallHeight * 0.5f;
            _dotPreview.transform.position = new Vector3(closestPoint.x, windowHeight, closestPoint.z);
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);

        if (nearestWall != null)
        {
            float windowHeight = AppHelper._wallHeight * 0.5f;
            Vector3 finalPos = new Vector3(closestPoint.x, windowHeight, closestPoint.z);

            GameObject windowDot = GameObject.Instantiate(
                _dotPreview,
                finalPos,
                Quaternion.identity,
                nearestWall.transform
            );
            windowDot.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Window window = windowDot.AddComponent<Window>();
            window.Initialize(nearestWall, finalPos);

            Debug.Log($"Window opening placed on {nearestWall.name} at {finalPos}");
        }

        GameManager.Instance.GetSubStateManager().SetSubState(new Ortho_IdleState(GameManager.Instance.GetOrthoCamera()));
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

    private void GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point, out Vector3 closest)
    {
        Vector3 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) { closest = a; return; }
        float t = Vector3.Dot(point - a, ab) / len2;
        t = Mathf.Clamp01(t);
        closest = a + ab * t;
    }

    private Wall FindFirstWall()
    {
        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if (wall != null)
                return wall;
        }
        return null;
    }

    private void PlaceWindow(Wall wall, Vector3 position)
    {
        if (_dotPrefab == null)
        {
            Debug.LogWarning("Dot prefab not set, skipping window placement.");
            return;
        }

        float windowHeight = AppHelper._wallHeight * 0.5f;
        Vector3 finalPos = new Vector3(position.x, windowHeight, position.z);

        GameObject windowDot = GameObject.Instantiate(
            _dotPrefab,
            finalPos,
            Quaternion.identity,
            wall.transform
        );

        windowDot.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Window window = windowDot.AddComponent<Window>();
        window.Initialize(wall, finalPos);

        Debug.Log($"Window opening automatically placed on {wall.name} at {finalPos}");
    }

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }
}
