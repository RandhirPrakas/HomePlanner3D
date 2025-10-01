using System.Collections.Generic;
using UnityEngine;

public class MoveRoomState : ICameraSubState
{
    private Room _activeRoom;

    private Room ActiveRoom
    {
        get => _activeRoom;
        set
        {
            if (_activeRoom != value)
            {
                _activeRoom = value;
                RoomManager.Instance.SetActiveRoom(value);
            }
        }
    }


    private Vector3 _lastMousePosition;
    private bool _isDragging = false;

    private Dictionary<WallPoint, Vector3> _originalPositions = new Dictionary<WallPoint, Vector3>();
    
    private OrthoCam _orthoCam;

    public MoveRoomState(Room activeRoom, OrthoCam orthoCam)
    {
        SetActiveRoom(activeRoom);
        _orthoCam = orthoCam;
    }

    public void SetActiveRoom(Room room)
    {
        ActiveRoom = room;

        _originalPositions.Clear();
        foreach (WallPoint wp in ActiveRoom._roomWallPoints)
        {
            _originalPositions[wp] = wp._position;
        }

        Debug.Log($"Active room: {ActiveRoom.name}");
    }

    public void Enter()
    {
        _isDragging = false;
    }

    public void Exit()
    {
        ActiveRoom = null;
        _originalPositions.Clear();
        _isDragging = false;
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        if (ActiveRoom == null) return;

        _lastMousePosition = worldPos;
        _isDragging = true;


        ActiveRoom.MeshCollider.enabled = false;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging || ActiveRoom == null) return;

        Vector3 delta = worldPos - _lastMousePosition;
        delta.y = 0;

        foreach (WallPoint wp in ActiveRoom._roomWallPoints)
        {
            wp.SetPosition(wp._position + delta);
        }

        HashSet<Wall> wallsToUpdate = new HashSet<Wall>();
        foreach (WallPoint wp in ActiveRoom._roomWallPoints)
        {
            foreach (Wall wall in wp.GetConnectedWalls())
            {
                wallsToUpdate.Add(wall);
            }
        }

        ActiveRoom.UpdateFloor();
        foreach (Wall wall in wallsToUpdate)
        {
            wall.UpdateFromPoints();
        }

        _lastMousePosition = worldPos;
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        _isDragging = false;

        if (ActiveRoom != null)
        {
            ActiveRoom.UpdateCollider();
        }

        _originalPositions.Clear();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnPinch(float delta) {}
}
