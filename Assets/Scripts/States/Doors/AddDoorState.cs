using UnityEditor;
using UnityEngine;

public class AddDoorState : ICameraSubState
{
    private Wall _targetWall;
    private GameObject _dotPreview;
    private float _tOnWall;
    private GameObject _dotPrefab;

    public AddDoorState(Wall wall)
    {
        _targetWall = wall;
        _dotPrefab = Resources.Load<GameObject>("Prefabs/DoorDotPrefab");
    }

    public void Enter()
    {
        Debug.Log("Entered AddDoorState");

        if (_targetWall == null)
        {
            Debug.LogWarning("No wall provided to AddDoorState! Exiting to IdleState.");
            GameManager.Instance.SetSubState(new Ortho_IdleState()); // Go to a safe state
            return;
        }

        // --- Create the preview object ---
        if (_dotPrefab != null)
        {
            _dotPreview = GameObject.Instantiate(_dotPrefab);
        }
        else
        {
            _dotPreview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _dotPreview.name = "Door Preview";
            _dotPreview.transform.localScale = Vector3.one;
            var col = _dotPreview.GetComponent<Collider>();
            if (col) GameObject.Destroy(col);
        }

        // --- FIX: Immediately move the preview to the center of the wall ---
        // This prevents it from ever appearing at (0,0,0).
        Vector3 start = _targetWall.GetStartPosition();
        Vector3 end = _targetWall.GetEndPosition();
        Vector3 midPoint = Vector3.Lerp(start, end, 0.5f); // Find midpoint
        _dotPreview.transform.position = new Vector3(midPoint.x, 0.1f, midPoint.z);
    }

    public void Exit()
    {
        Debug.Log("Exited AddDoorState");

        if (_dotPreview != null)
        {
            GameObject.Destroy(_dotPreview);
        }
    }

    public void Update() { }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        Debug.Log("Initialized AddDoorState");
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        // This logic is now identical to OnTouchHold, which is fine.
        // It ensures the preview snaps to the touch position immediately.
        UpdatePreviewPosition(worldPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        UpdatePreviewPosition(worldPos);
    }

    /// <summary>
    /// Helper method to keep code DRY (Don't Repeat Yourself).
    /// Updates the preview dot's position based on the user's touch.
    /// </summary>
    private void UpdatePreviewPosition(Vector3 worldPos)
    {
        if (_dotPreview == null) return;

        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);

        if (nearestWall != null)
        {
            _targetWall = nearestWall;

            _dotPreview.transform.position = new Vector3(closestPoint.x, 0.1f, closestPoint.z);
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        Wall nearestWall = FindNearestWall(worldPos, out Vector3 closestPoint);

        if (nearestWall != null)
        {
            // Create a simple dot prefab at the position
            GameObject doorDot = GameObject.Instantiate(
                _dotPreview,
                closestPoint,
                Quaternion.identity,
                nearestWall.transform
            );

            /*if (_dotPrefab == null)
            {
                doorDot.transform.localScale = Vector3.one;
                var col = doorDot.GetComponent<Collider>();
                if (col) GameObject.Destroy(col);
            }*/

            // Add Opening component
            Opening opening = doorDot.AddComponent<Opening>();
            opening.Initialize(nearestWall, closestPoint, OpeningType.Door);

            Debug.Log($"Door opening placed on {nearestWall.name} at {closestPoint}");
        }

        // Switch back to the safe IdleState
        GameManager.Instance.GetSubStateManager().SetSubState(new Ortho_IdleState());
    }


    private Wall FindNearestWall(Vector3 point, out Vector3 closestPoint, float snapThreshold = 5f)
    {
        Wall nearest = null;
        float minDist = float.MaxValue;
        closestPoint = point;

        foreach (Room room in RoomManager.Instance._allRooms)
        {
           /* if (room == null || room._allRoomWalls == null) continue;

            foreach (Wall wall in room._allRoomWalls)
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
            }*/
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
}
