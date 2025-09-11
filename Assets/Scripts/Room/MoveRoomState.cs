using System.Collections.Generic;
using UnityEngine;

public class MoveRoomState : ICameraSubState
{
    private Room _activeRoom;
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
        _activeRoom = room;

        _originalPositions.Clear();
        foreach (WallPoint wp in _activeRoom._roomWallPoints)
        {
            _originalPositions[wp] = wp._position;
        }

        Debug.Log($"Active room: {_activeRoom.name}");
    }

    public void Enter()
    {
        _isDragging = false;
    }

    public void Exit()
    {
        _activeRoom = null;
        _originalPositions.Clear();
        _isDragging = false;
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        if (_activeRoom == null) return;

        _lastMousePosition = worldPos;
        _isDragging = true;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging || _activeRoom == null) return;

        Vector3 delta = worldPos - _lastMousePosition;
        delta.y = 0;

        // Move all wall points of the room
        foreach (WallPoint wp in _activeRoom._roomWallPoints)
        {
            wp.SetPosition(wp._position + delta);
        }

        // Update room floor mesh
        _activeRoom.UpdateFloor();

        // Update all walls connected to moved points
        foreach (WallPoint wp in _activeRoom._roomWallPoints)
        {
            foreach (Wall wall in wp.GetConnectedWalls())
            {
                // Only update walls that are not part of other rooms sharing the point
                if (wall.GetRoomParent() == _activeRoom)
                    wall.UpdateFromPoints(true);
                else
                    wall.UpdateFromPoints();
            }
        }

        _lastMousePosition = worldPos;
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        _isDragging = false;
        _activeRoom = null;
        _originalPositions.Clear();
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnPinch(float delta) { /* Optional: zoom room camera */ }
}
