using UnityEditor;
using UnityEngine;

public class AddDoorState : ICameraSubState
{
    private Wall _targetWall;
    private GameObject _dotPreview;
    private float _tOnWall;
    private GameObject _dotPrefab;

    private OrthoCam _orthoCam;

    public AddDoorState(OrthoCam orthoCam)
    {
        _dotPrefab = Resources.Load<GameObject>("Prefabs/DoorDotPrefab");
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
            _dotPreview = GameObject.Instantiate(_dotPrefab);
        }
        else
        {
            _dotPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _dotPreview.name = "Door Preview";
            _dotPreview.transform.localScale = Vector3.one;
            _dotPreview.tag = "Door";
        }

        // --- Move preview to midpoint of target wall ---
        Vector3 start = _targetWall.GetStartPosition();
        Vector3 end = _targetWall.GetEndPosition();
        Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
        _dotPreview.transform.position = new Vector3(midPoint.x, 0.5f, midPoint.z);

        PlaceDoor(_targetWall, midPoint);
    }


    public void Exit()
    {
        Debug.Log("Exited AddDoorState");

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

            _dotPreview.transform.position = new Vector3(closestPoint.x, 3f, closestPoint.z);
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);

        closestPoint.y = 3f;
        if (nearestWall != null)
        {
            // Create a simple dot prefab at the position
            GameObject doorDot = GameObject.Instantiate(
                _dotPreview,
                closestPoint,
                Quaternion.identity,
                nearestWall.transform
            );
            doorDot.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Door door = doorDot.AddComponent<Door>();
            door.Initialize(nearestWall, closestPoint);

            Debug.Log($"Door opening placed on {nearestWall.name} at {closestPoint}");
        }

        // Switch back to the safe IdleState
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

    // Get Distance from touch point to the nearest wall
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
        foreach(Wall wall in WallManager.Instance._allWalls)
        {
            if (wall != null)
                return wall;
        }

        return null;
    }

    private void PlaceDoor(Wall wall, Vector3 position)
    {
        if (_dotPrefab == null)
        {
            Debug.LogWarning("Dot prefab not set, skipping door placement.");
            return;
        }

        GameObject doorDot = GameObject.Instantiate(
            _dotPrefab,
            position,
            Quaternion.identity,
            wall.transform
        );

        doorDot.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Door door = doorDot.AddComponent<Door>();
        door.Initialize(wall, position);

        door.transform.position = new Vector3(position.x, 0.5f, position.z);
        Debug.Log($"Door opening automatically placed on {wall.name} at {position}");
    }

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }
}
