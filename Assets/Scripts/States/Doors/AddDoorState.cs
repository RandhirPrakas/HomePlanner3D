using UnityEditor;
using UnityEngine;

public class AddDoorState : ICameraSubState
{
    private Wall _targetWall;
    private GameObject _dotPreview;
    private GameObject _dotPrefab;

    private OrthoCam _orthoCam;

    public AddDoorState(OrthoCam orthoCam)
    {
        _dotPrefab = Resources.Load<GameObject>("Prefabs/Door/DoorVisualizer");
        _orthoCam = orthoCam;
    }

    public void Enter()
    {
        Debug.Log("Entered AddDoorState");

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
            // create preview but don't attach Door script yet
            _dotPreview = GameObject.Instantiate(_dotPrefab);
            _dotPreview.name = "Door Preview";
        }
        else
        {
            Debug.LogError("Dot prefab not found at Resources/Prefabs/Door/DoorVisualizer");
        }

        // --- Position preview at midpoint of target wall initially ---
        Vector3 start = _targetWall.GetStartPosition();
        Vector3 end = _targetWall.GetEndPosition();
        Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
        _dotPreview.transform.position = new Vector3(midPoint.x, 0.5f, midPoint.z);
        _dotPreview.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void Exit()
    {
        Debug.Log("Exited AddDoorState");

        if (_dotPreview != null)
        {
            GameObject.Destroy(_dotPreview);
            _dotPreview = null;
        }
    }

    public void Update()
    {
        _orthoCam.Update();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        Debug.Log("Initialized AddDoorState");
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = 0.5f;
        UpdatePreviewPosition(worldPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        UpdatePreviewPosition(worldPos);
    }

    /// <summary>
    /// Updates the preview dot's position based on the user's touch.
    /// </summary>
    private void UpdatePreviewPosition(Vector3 worldPos)
    {
        if (_dotPreview == null) return;

        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);

        if (nearestWall != null)
        {
            _targetWall = nearestWall;
            _dotPreview.transform.position = new Vector3(closestPoint.x, 0.5f, closestPoint.z);
            _dotPreview.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            SetDoorVisualRotation(_dotPreview, nearestWall);
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);
        closestPoint.y = 3f;

        if(!CanPlaceDoor(nearestWall, closestPoint))
        {
            Debug.Log("Cannot Place Door as it is too close to other opening or wall end");
            return;
        }

        if (nearestWall != null && _dotPrefab != null)
        {
            // create the actual placed door
            GameObject doorSprites = GameObject.Instantiate(
                _dotPrefab,
                closestPoint,
                Quaternion.Euler(90f, 0f, 0f),
                nearestWall.transform
            );
            GameObject doorVisual = new GameObject("Door");
            doorVisual.transform.position = closestPoint;
            doorVisual.transform.SetParent(nearestWall.transform);
            doorVisual.tag = "Door";

            doorSprites.transform.SetParent(doorVisual.transform);

            Door door = doorVisual.AddComponent<Door>();
            door.Initialize(nearestWall, closestPoint);
            door.OpeningVisual = doorSprites;

            SetDoorVisualRotation(door.OpeningVisual, nearestWall);
            Debug.Log($"Door opening placed on {nearestWall.name} at {closestPoint}");
        }

        // destroy preview
        if (_dotPreview != null)
        {
            GameObject.Destroy(_dotPreview);
            _dotPreview = null;
        }

        // Switch back to IdleState
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
            GetClosesDistance(a, b, point, out proj);

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

    private float GetClosesDistance(Vector3 a, Vector3 b, Vector3 point, out Vector3 closest)
    {
        Vector3 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) { closest = a; return 0f; }
        float t = Vector3.Dot(point - a, ab) / len2;
        t = Mathf.Clamp01(t);
        closest = a + ab * t;
        return t;
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

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }

    private void SetDoorVisualRotation(GameObject doorPriview, Wall wall)
    {
        Vector3 a = wall.GetStartPosition();
        Vector3 b = wall.GetEndPosition();

        // Wall direction (along the wall)
        Vector3 wallDir = (b - a).normalized;

        // direction perpednicular to this wal l
        Vector3 perp = Vector3.Cross(wallDir, Vector3.up).normalized;

        Quaternion targetRot = Quaternion.LookRotation(perp, Vector3.up);

        Quaternion fixRot = Quaternion.Euler(90f, 0f, 0f);
        doorPriview.transform.rotation = targetRot * fixRot;
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

        foreach(Opening opening in wall._allOpenings)
        {
            if ((AppHelper.GetXZDistanceBetweenTwoVector(currentPosition, opening.OpeningPosition) < (opening.Width) / 2 + (AppHelper._doorWidth / 2) + 0.25f))
                return false;
        }
        return true;
    }
}
