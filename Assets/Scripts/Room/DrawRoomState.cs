using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawRoomState : ICameraSubState
{
    private Vector3 _startPos;
    private Room _currentRoom;
    private ProceduarlwallGenerator _wallGenerator;
    private LineRenderer _wallOutline;
    private Grid _grid;
    private bool _isNewRoomMode;
    private bool _roomLocked;

    // --- Tunables ---
    private const float CornerEpsilon = 0.001f; // tolerance for comparing Vector3 corners

    private Vector3 _snappedEnd;

    public DrawRoomState(Room existingRoom = null, bool isNewRoom = false)
    {
        _isNewRoomMode = isNewRoom;

        if (existingRoom != null)
        {
            _currentRoom = existingRoom;
            _roomLocked = true; 
        }
        else if (_isNewRoomMode)
        {
            CreateNewRoom();
            _roomLocked = true; // lock this room for all walls until state ends
        }
    }

    public void Enter()
    {
        if (_grid == null)
            _grid = GameObject.FindObjectOfType<Grid>();

        EnsureWallOutlineForCurrentRoom();

        if (_wallGenerator == null)
            _wallGenerator = new ProceduarlwallGenerator();

    }

    public void Exit()
    {
        Debug.Log("Exited DrawRoomState");
        _currentRoom = null;
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = 0.1f;

        if (WallPointManager.Instance._allWallPoints.Count == 0)
        {
            _startPos = _grid.GetNearestPointOnGrid(worldPos);
        }
        else
        {
            bool snappedToPoint = false;
            foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
            {
                if (AppHelper.CanSnapPoint(worldPos + Vector3.up * AppHelper._lrYPos, wp._position))
                {
                    _startPos = wp._position;
                    snappedToPoint = true;
                    break;
                }
            }

            if (!snappedToPoint)
                _startPos = _grid.GetNearestPointOnGrid(worldPos);
        }

        _startPos.y = AppHelper._lrYPos;

        if (_currentRoom == null)
            CreateNewRoom();

        EnsureWallOutlineForCurrentRoom();

        _wallOutline.positionCount = 1;
        _wallOutline.SetPosition(0, _startPos);
    }


    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_wallOutline == null) return;

        if (_wallOutline.positionCount != 2)
        {
            _wallOutline.positionCount = 2;
            _wallOutline.SetPosition(0, _startPos);
        }

        worldPos.y = 0.1f;
        worldPos = AppHelper.SmartSnapToAxis(worldPos, WallPointManager.Instance._allWallPoints);
        worldPos = AppHelper.WrapPosition(_startPos, worldPos);

        _wallOutline.SetPosition(1, worldPos + Vector3.up * AppHelper._lrYPos);
    }


    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (Vector3.Distance(_startPos, worldPos) < AppHelper._minimumWallLength)
        {
            Debug.Log("Not Enough Points");
            if (_wallOutline != null && _wallOutline.positionCount > 0)
                _wallOutline.positionCount -= 1;
            return;
        }

        // --- Snap end point ---
        if (WallPointManager.Instance._allWallPoints.Count == 0)
        {
            _snappedEnd = _grid.GetNearestPointOnGrid(worldPos);
        }
        else
        {
            bool snappedToPoint = false;
            foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
            {
                if (AppHelper.CanSnapPoint(worldPos + Vector3.up * AppHelper._lrYPos, wp._position))
                {
                    _snappedEnd = wp._position;
                    snappedToPoint = true;
                    break;
                }
            }

            if (!snappedToPoint)
            {
                _snappedEnd = _grid.GetNearestPointOnGrid(worldPos);
            }
            else
            {
                _snappedEnd = AppHelper.SmartSnapToAxis(_snappedEnd, WallPointManager.Instance._allWallPoints);
                _snappedEnd = AppHelper.WrapPosition(_startPos, _snappedEnd);
            }
        }

        _snappedEnd.y = AppHelper._lrYPos;

        Room roomAtStart = null;
        Room roomAtEnd = null;

        // --- Handle New Room Mode ---
        if (_isNewRoomMode)
        {
            if (_currentRoom == null || !_roomLocked)
            {
                CreateNewRoom();
                _roomLocked = true; // lock so only one room is created in this mode
            }
        }
        else
        {
            roomAtStart = FindRoomByPoint(_startPos);
            roomAtEnd = FindRoomByPoint(_snappedEnd);
        }

        // --- Handle merging / assigning ---
        if (roomAtStart != null && roomAtEnd != null)
        {
            if (roomAtStart != roomAtEnd)
            {
                // Only merge if this new wall closes a loop between rooms
                if (DoesClosingWallCompleteLoop(roomAtStart, roomAtEnd, _startPos, _snappedEnd))
                {
                    MergeRooms(roomAtStart, roomAtEnd);
                    _currentRoom = roomAtStart;
                }
                else
                {
                    // keep them separate
                    _currentRoom = roomAtStart;
                }
            }
            else
            {
                _currentRoom = roomAtStart;
            }
        }
        else if (roomAtStart != null)
        {
            _currentRoom = roomAtStart;
        }
        else if (roomAtEnd != null)
        {
            _currentRoom = roomAtEnd;
        }
        else if (!_isNewRoomMode) // ✅ Only auto-create in normal mode
        {
            CreateNewRoom();
        }

        // --- Ensure wall outline + draw ---
        EnsureWallOutlineForCurrentRoom();
        DrawSingleWall(_snappedEnd);
    }



    private void DrawSingleWall(Vector3 position)
    {
        if (_currentRoom == null)
            CreateNewRoom();

        GameObject wallGO = new GameObject("Wall");
        wallGO.transform.SetParent(_currentRoom.transform, true);

        Wall wallComp = wallGO.AddComponent<Wall>();

        // Create/Get wall points
        WallPoint startWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(_startPos, "StartWallPoint");

        position = AppHelper.SmartSnapToAxis(position, WallPointManager.Instance._allWallPoints);
        position = AppHelper.WrapPosition(_startPos, position);

        WallPoint endWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(position + Vector3.up * AppHelper._lrYPos, "EndWallPoint");

        startWallPoint.transform.SetParent(wallComp.transform, true);
        endWallPoint.transform.SetParent(wallComp.transform, true);

        startWallPoint._connectedWalls.Add(wallComp);
        endWallPoint._connectedWalls.Add(wallComp);

        wallComp.SetStartAndEndPosition(startWallPoint, endWallPoint, _currentRoom);

        _currentRoom._allRoomWalls.Add(wallComp);

        AddCornerUnique(_currentRoom._wallCorners, startWallPoint._position);
        AddCornerUnique(_currentRoom._wallCorners, endWallPoint._position);

        ResetWallOutlineBase();

        Debug.Log($"[Room: {_currentRoom.name}] Wall created between {_startPos} and {position}");

        AppHelper.InvokeOnWallCreation();
    }


    private void ResetWallOutlineBase()
    {
        if (_wallOutline != null)
            _wallOutline.positionCount = 0;
    }

    private void CreateNewRoom()
    {
        // If there is a current room but it is "empty" (no walls) — remove it safely.
        if (_currentRoom != null)
        {
            // Use the logical wall list rather than transform.childCount
            if (_currentRoom._allRoomWalls == null || _currentRoom._allRoomWalls.Count == 0)
            {
                // If the cached _wallOutline points to that room, clear it first
                if (_wallOutline != null && _wallOutline.gameObject == _currentRoom.gameObject)
                    _wallOutline = null;

                RoomManager.Instance._allRooms.Remove(_currentRoom);
                GameObject.Destroy(_currentRoom.gameObject);
                _currentRoom = null;
            }
        }

        // Create a new room
        GameObject roomGO = new GameObject("Room");
        Room newRoom = roomGO.AddComponent<Room>();
        RoomManager.Instance._allRooms.Add(newRoom);
        newRoom.SpawnWallLabelCanvas();

        _currentRoom = newRoom;

        // Ensure outline is created on this room (and cached properly)
        EnsureWallOutlineForCurrentRoom();

        Debug.Log($"Started New Room: {_currentRoom.name}");
    }


    private void EnsureWallOutlineForCurrentRoom()
    {
        if (_currentRoom == null)
        {
            _wallOutline = null;
            return;
        }

        // Try to find an existing LineRenderer on the current room GameObject
        LineRenderer existing = _currentRoom.gameObject.GetComponent<LineRenderer>();
        if (existing == null)
        {
            // Create one if missing
            existing = _currentRoom.gameObject.AddComponent<LineRenderer>();
            existing.positionCount = 0;
            existing.material = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial");
            existing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            existing.startWidth = AppHelper._lrThickness;
            existing.endWidth = AppHelper._lrThickness;
        }

        // Always update the cached field to the RL that actually lives on the current room
        _wallOutline = existing;
    }


    private Room FindRoomByPoint(Vector3 position)
    {
        foreach (Room room in RoomManager.Instance._allRooms)
        {
            if (room == null) continue;

            foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
            {
                if (wp == null) continue;
                if (!AppHelper.CanSnapPoint(position, wp._position))
                    continue;

                foreach (Wall wall in wp._connectedWalls)
                {
                    if (wall != null && wall.GetParentRoom() == room)
                        return room;
                }
            }
        }
        return null;
    }

    private void MergeRooms(Room intoRoom, Room fromRoom)
    {
        if (intoRoom == null || fromRoom == null || intoRoom == fromRoom) return;

        var wallsToMove = new List<Wall>(fromRoom._allRoomWalls);
        foreach (var wall in wallsToMove)
        {
            if (wall == null) continue;

            wall.transform.SetParent(intoRoom.transform, true);
            wall.SetParentRoom(intoRoom);

            if (!intoRoom._allRoomWalls.Contains(wall))
                intoRoom._allRoomWalls.Add(wall);
        }

        foreach (var c in fromRoom._wallCorners)
            AddCornerUnique(intoRoom._wallCorners, c);

        if (_wallOutline != null && _wallOutline.gameObject == fromRoom.gameObject)
            _wallOutline = null;

        RoomManager.Instance._allRooms.Remove(fromRoom);
        GameObject.Destroy(fromRoom.gameObject);

        Debug.Log($"Merged rooms. Now using: {intoRoom.name}");
    }

    private void AddCornerUnique(HashSet<Vector3> set, Vector3 v)
    {
        foreach (var existing in set)
        {
            if (AlmostEqual(existing, v))
                return;
        }
        set.Add(v);
    }

    private bool AlmostEqual(Vector3 a, Vector3 b, float eps = CornerEpsilon)
    {
        return (a - b).sqrMagnitude <= eps * eps;
    }

    // ----------- NEW HELPER ------------
    private bool DoesClosingWallCompleteLoop(Room r1, Room r2, Vector3 start, Vector3 end)
    {
        bool r1HasStart = r1.HasCornerNear(start);
        bool r1HasEnd = r1.HasCornerNear(end);
        bool r2HasStart = r2.HasCornerNear(start);
        bool r2HasEnd = r2.HasCornerNear(end);

        // Only true if both rooms already "touch" these endpoints → adding this wall closes a loop
        return (r1HasStart && r2HasEnd) || (r1HasEnd && r2HasStart);
    }

    public void Init(Vector3 worldPos, Vector2 screenPos) { }

    private Vector3 SnapEndPoint(Vector3 worldPos)
    {
        if (WallPointManager.Instance._allWallPoints.Count == 0)
            return _grid.GetNearestPointOnGrid(worldPos);

        foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
        {
            if (AppHelper.CanSnapPoint(worldPos + Vector3.up * AppHelper._lrYPos, wp._position))
                return wp._position;
        }

        return _grid.GetNearestPointOnGrid(worldPos);
    }

    private Room CreateNewRoomAndReturn()
    {
        CreateNewRoom(); // your existing method
        return _currentRoom;
    }

}
