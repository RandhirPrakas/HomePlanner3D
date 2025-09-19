using System.Collections.Generic;
using UnityEngine;

public class MoveWallState : ICameraSubState
{
    private Wall _activeWall;
    public Wall ActiveWall
    {
        get => _activeWall;
        set
        {
            if (_activeWall == value) return;
            _activeWall = value;
            if (WallManager.Instance != null)
                WallManager.Instance._currentSelectedWall = _activeWall;
        }
    }

    private EditUI _editUI;
    private Vector3 _lastWallPosition;
    private bool _isDragging = false;
    private Vector3 _direction;
    private Color _defaultColor = Color.white;
    private OrthoCam _orthoCam;

    private readonly HashSet<Room> _roomsToUpdate = new HashSet<Room>();
    private readonly HashSet<Wall> _wallsToUpdate = new HashSet<Wall>();

    public MoveWallState(Wall wall, OrthoCam orthoCam)
    {
        SetActiveWall(wall);
        _orthoCam = (orthoCam == null) ? GameManager.Instance.GetOrthoCamera() : orthoCam;
    }

    public void SetActiveWall(Wall wall, bool canChangeColor = true)
    {
        if (ActiveWall != null && ActiveWall == wall) return;

        if (wall != null) ActiveWall = wall;

        if (GameManager.Instance != null) GameManager.Instance._activeWall = _activeWall;

        // NOTE: This assumes WallPoint and Wall have public properties.
        // If not, revert to GetEndPosition() / GetStartPosition().
        Vector3 wallVector = _activeWall.EndWallPoint.transform.position - _activeWall.StartWallPoint.transform.position;
        _direction = new Vector3(-wallVector.z, 0, wallVector.x).normalized;

        // This line can cause errors if the material path is wrong or in builds.
        // It's better to assign this material via the inspector.
        _defaultColor = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial").color;

        if (canChangeColor)
            _activeWall.GetComponent<LineRenderer>().material.color = Color.blue;

        SetEditUI();
    }

    public void Enter()
    {
        _isDragging = false;
    }

    public void Exit()
    {
        if (ActiveWall != null)
        {
            ActiveWall.GetComponent<LineRenderer>().material.color = _defaultColor;
        }
        DestroyEditUI();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_activeWall != null)
            {
                WallManager.Instance.DeleteWall(_activeWall);
                // After deletion, this state is invalid and should be exited.
                // You might need to add state transition logic here.
            }
        }
        if (_orthoCam != null)
            _orthoCam.Update();
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        SetActiveWall(GetActiveWall(screenPos));
        if (_activeWall != null)
        {
            _lastWallPosition = worldPos;
            _isDragging = true;
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging || _activeWall == null) return;

        worldPos.y = 0;
        Vector3 delta = worldPos - _lastWallPosition;
        delta.y = 0;

        Vector3 distance = Vector3.Dot(delta, _direction) * _direction;
        if (distance.magnitude > 0.01f)
        {
            MoveWall(distance);
            _lastWallPosition = worldPos;
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        _isDragging = false;
        if (ActiveWall != null)
        {
            ActiveWall.GetComponent<LineRenderer>().material.color = _defaultColor;
            HandleSnappingOnMoveEnd();
        }
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPinch(float delta)
    {
        if (_orthoCam != null)
            _orthoCam.ZoomCamera(delta);
    }

    private void DestroyEditUI()
    {
        if (_editUI != null)
        {
            GameObject.Destroy(_editUI.gameObject);
            _editUI = null;
        }
    }

    private void MoveWall(Vector3 positionOffset)
    {
        _roomsToUpdate.Clear();
        _wallsToUpdate.Clear();

        _activeWall.StartWallPoint.SetPosition(_activeWall.StartWallPoint._position + positionOffset);
        _activeWall.EndWallPoint.SetPosition(_activeWall.EndWallPoint._position + positionOffset);

       
        var pointsToCheck = new List<WallPoint> { _activeWall.StartWallPoint, _activeWall.EndWallPoint };
        foreach (var point in pointsToCheck)
        {
            foreach (var wall in point.GetConnectedWalls())
            {
                _wallsToUpdate.Add(wall);
                foreach (var room in wall.GetRoomParent())
                {
                    if (room != null) _roomsToUpdate.Add(room);
                }
            }
        }

        
        foreach (var wall in _wallsToUpdate)
        {
            wall.UpdateFromPoints(true);
        }

        foreach (var room in _roomsToUpdate)
        {
            room.UpdateFloor();
        }

        if (_editUI != null)
            _editUI.transform.position += positionOffset;
    }

    private Wall GetActiveWall(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                // GetComponentInParent is safer if the collider is on a child object.
                Wall wall = hit.collider.GetComponentInParent<Wall>();
                if (wall != null && wall != _activeWall)
                {
                    return wall;
                }
            }
        }
        return _activeWall; // Return the current active wall if nothing new is hit
    }

    private void SetEditUI()
    {

        if (ActiveWall == null) return;

        Vector3 yOffset = Vector3.up * 1f;
        Vector3 zOffset = GetPerpendicularDirection(ActiveWall.GetStartPosition(), ActiveWall.GetEndPosition()) * -5f;

        Vector3 midPoint = (ActiveWall.GetStartPosition() + ActiveWall.GetEndPosition()) / 2;

        Vector3 position = midPoint + yOffset + zOffset;

        if (_editUI == null)
        {
            // Instantiate and parent under the wall point
            _editUI = GameObject.Instantiate(
                GameManager.Instance._uiManager._editUIPrefab,
                position,
                Quaternion.identity,
                ActiveWall.transform
            );
            _editUI.gameObject.name = "EditUI";
        }
        else
        {
            _editUI.transform.SetParent(ActiveWall.transform, false);
            _editUI.transform.position = position;
        }

        _editUI.Initialize(EditUIType.WallEdit);
    }

    private Vector3 GetPerpendicularDirection(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        return new Vector3(-dir.z, 0, dir.x);
    }

    private void HandleSnappingOnMoveEnd()
    {
        if (ActiveWall == null) return;

        WallPoint startPoint = ActiveWall.StartWallPoint;
        WallPoint endPoint = ActiveWall.EndWallPoint;

        // 1. Find the nearest potential snap target for EACH endpoint of the wall.
        // We pass the point itself to exclude it from the search.
        WallPoint startSnapTarget = WallPointManager.Instance.GetExistingPointAt(startPoint._position, startPoint);
        WallPoint endSnapTarget = WallPointManager.Instance.GetExistingPointAt(endPoint._position, endPoint);

        // If neither end has a valid target, there's nothing to do.
        if (startSnapTarget == null && endSnapTarget == null)
        {
            return;
        }

        // 2. Determine which endpoint is closer to its respective snap target.
        float startSnapDistance = (startSnapTarget != null)
            ? Vector3.Distance(startPoint._position, startSnapTarget._position)
            : float.MaxValue;

        float endSnapDistance = (endSnapTarget != null)
            ? Vector3.Distance(endPoint._position, endSnapTarget._position)
            : float.MaxValue;

        WallPoint pointToSnap;
        WallPoint pointToShift;
        WallPoint snapTarget;

        if (startSnapDistance <= endSnapDistance)
        {
            // The start point is closer to its target, so it will be the one to snap.
            pointToSnap = startPoint;
            pointToShift = endPoint;
            snapTarget = startSnapTarget;
        }
        else
        {
            // The end point is closer to its target.
            pointToSnap = endPoint;
            pointToShift = startPoint;
            snapTarget = endSnapTarget;
        }

        // 3. Calculate the vector needed to move the snapping point to its target.
        Vector3 snapVector = snapTarget._position - pointToSnap._position;

        // 4. Apply the transformations.
        // IMPORTANT: Shift the second point FIRST, before merging the snapping point.
        // Merging destroys the 'pointToSnap' object, so we need its position data before it's gone.
        pointToShift.SetPosition(pointToShift._position + snapVector);

        // Now, merge the primary point with its target.
        pointToSnap.MergeWith(snapTarget);

        // 5. Explicitly update all walls connected to the shifted point to redraw them.
        // (The MergeWith function already handles updates for its connected walls).
        foreach (var wall in pointToShift.GetConnectedWalls())
        {
            wall.UpdateFromPoints();
        }

        // After the merge, the ActiveWall reference might be connected to a new point.
        // To be safe, we can nullify it so the state doesn't hold a stale reference.
        ActiveWall = null;
    }
}