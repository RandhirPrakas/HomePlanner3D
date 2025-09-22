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

        if (GameManager.Instance != null) GameManager.Instance._activeWall = ActiveWall;

        // NOTE: This assumes WallPoint and Wall have public properties.
        // If not, revert to GetEndPosition() / GetStartPosition().
        Vector3 wallVector = _activeWall.EndWallPoint.transform.position - ActiveWall.StartWallPoint.transform.position;
        _direction = new Vector3(-wallVector.z, 0, wallVector.x).normalized;

        // This line can cause errors if the material path is wrong or in builds.
        // It's better to assign this material via the inspector.
        _defaultColor = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial").color;

        if (canChangeColor)
            ActiveWall.GetComponent<LineRenderer>().material.color = Color.blue;

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
            if (ActiveWall != null)
            {
                WallManager.Instance.DeleteWall(ActiveWall);
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
        if (ActiveWall != null)
        {
            _lastWallPosition = worldPos;
            _isDragging = true;
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging || ActiveWall == null) return;

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
            UpdateRoomColliders();
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

        ActiveWall.StartWallPoint.SetPosition(ActiveWall.StartWallPoint._position + positionOffset);
        ActiveWall.EndWallPoint.SetPosition(ActiveWall.EndWallPoint._position + positionOffset);

       
        var pointsToCheck = new List<WallPoint> { ActiveWall.StartWallPoint, ActiveWall.EndWallPoint };
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
                if (wall != null && wall != ActiveWall)
                {
                    return wall;
                }
            }
        }
        return ActiveWall; // Return the current active wall if nothing new is hit
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

        WallPoint startSnapTarget = WallPointManager.Instance.GetExistingPointAt(startPoint._position, startPoint);
        WallPoint endSnapTarget = WallPointManager.Instance.GetExistingPointAt(endPoint._position, endPoint);

        if (startSnapTarget == null && endSnapTarget == null)
        {
            return;
        }

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
            pointToSnap = startPoint;
            pointToShift = endPoint;
            snapTarget = startSnapTarget;
        }
        else
        {
            pointToSnap = endPoint;
            pointToShift = startPoint;
            snapTarget = endSnapTarget;
        }

        Vector3 snapVector = snapTarget._position - pointToSnap._position;

        pointToShift.SetPosition(pointToShift._position + snapVector);

        pointToSnap.MergeWith(snapTarget);

        foreach (var wall in pointToShift.GetConnectedWalls())
        {
            wall.UpdateFromPoints();
        }

        ActiveWall = null;
    }

    private void UpdateRoomColliders()
    {
        foreach(Room room in _roomsToUpdate)
        {
            room.UpdateCollider();
        }
    }

}