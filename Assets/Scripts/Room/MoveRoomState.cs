using System.Collections.Generic;
using System.Linq;
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
                if (RoomManager.Instance != null) RoomManager.Instance.SetActiveRoom(value);
            }
        }
    }

    private Room _originalRoom;
    private bool _isDragging = false;
    private Vector3 _lastDragPosition;
    private OrthoCam _orthoCam;

    public MoveRoomState(Room roomToMove, OrthoCam orthoCam)
    {
        _originalRoom = roomToMove;
        _orthoCam = orthoCam;
    }

    public void Enter()
    {
        if (_originalRoom == null) return;
        Room roomClone = CreateDetachedClone(_originalRoom);
        ActiveRoom = roomClone;
        _originalRoom.gameObject.SetActive(false);
        _isDragging = false;
    }

    /*public void Exit()
    {
        if (ActiveRoom != null)
        {
            const float snapTolerance = 0.1f;
            var clonePoints = new List<WallPoint>(ActiveRoom._roomWallPoints);
            foreach (WallPoint clonePoint in clonePoints)
            {
                if (clonePoint == null) continue;
                WallPoint existingPointToMerge = WallPointManager.Instance.GetExistingPointAt(clonePoint._position, clonePoint);
                if (existingPointToMerge != null && Vector3.Distance(clonePoint._position, existingPointToMerge._position) < snapTolerance)
                {
                    clonePoint.MergeWith(existingPointToMerge);
                }
            }
        }
        if (_originalRoom != null)
        {
            _originalRoom.DestroyRoomAndCleanup();
        }
        if (RoomDetector.Instance != null && WallPointManager.Instance != null)
        {
            RoomDetector.Instance.DetectRooms(WallPointManager.Instance._allWallPoints);
        }
        ActiveRoom = null;
        _originalRoom = null;
        _isDragging = false;
    }*/
    public void Exit()
    {
        // --- Step 1: MERGE AND SNAP LOGIC ---
        // Finalize the clone's position and merge it with the existing geometry.
        if (ActiveRoom != null)
        {
            const float snapTolerance = 0.1f;
            var clonePoints = new List<WallPoint>(ActiveRoom._roomWallPoints);
            foreach (WallPoint clonePoint in clonePoints)
            {
                if (clonePoint == null) continue;
                WallPoint existingPointToMerge = WallPointManager.Instance.GetExistingPointAt(clonePoint._position, clonePoint);
                if (existingPointToMerge != null && Vector3.Distance(clonePoint._position, existingPointToMerge._position) < snapTolerance)
                {
                    clonePoint.MergeWith(existingPointToMerge);
                }
            }
        }

        // --- Step 2: REMOVE THE ORIGINAL ROOM ---
        // We no longer need the complex DestroyRoomAndCleanup method.
        // The RoomDetector will now handle all cleanup. We just need to remove the original GameObject.
        if (_originalRoom != null)
        {
            /*RoomManager.Instance._allRooms.Remove(_originalRoom);
            GameObject.Destroy(_originalRoom.gameObject);*/

            RoomManager.Instance.DeleteRoom(_originalRoom);
        }

        // --- Step 3: TRIGGER SCENE-WIDE REBUILD ---
        // With the geometry finalized, tell the RoomDetector to rebuild EVERYTHING.
        // It will destroy any old rooms and create a fresh, consistent set of new rooms.
        // This process will now correctly identify and ignore the "extra line" wall,
        // which will be cleaned up later if it becomes truly orphaned.
        if (RoomDetector.Instance != null && WallPointManager.Instance != null)
        {
            RoomDetector.Instance.DetectRooms(WallPointManager.Instance._allWallPoints);
        }

        // --- Step 4: CLEAR STATE ---
        ActiveRoom = null;
        _originalRoom = null;
        _isDragging = false;
    }

    public void Cancel()
    {
        if (ActiveRoom != null)
        {
            ActiveRoom.DestroyRoomAndCleanup();
        }
        if (_originalRoom != null)
        {
            _originalRoom.gameObject.SetActive(true);
        }
        ActiveRoom = null;
        _originalRoom = null;
        _isDragging = false;
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        if (ActiveRoom == null) return;
        _lastDragPosition = worldPos;
        _isDragging = true;
        if (ActiveRoom.MeshCollider != null) ActiveRoom.MeshCollider.enabled = false;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging || ActiveRoom == null) return;
        Vector3 delta = worldPos - _lastDragPosition;
        delta.y = 0;
        if (delta.sqrMagnitude == 0) return;

        foreach (WallPoint wp in ActiveRoom._roomWallPoints)
        {
            wp.SetPosition(wp._position + delta);
        }
        ActiveRoom.UpdateFloor();
        foreach (Wall wall in ActiveRoom._roomWalls)
        {
            wall.UpdateFromPoints();
        }
        _lastDragPosition = worldPos;
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (!_isDragging) return;
        _isDragging = false;
        if (ActiveRoom != null) ActiveRoom.UpdateCollider();
    }

    /*private Room CreateDetachedClone(Room original)
    {
        GameObject roomGO = new GameObject("Room_Clone");
        Room cloneRoom = roomGO.AddComponent<Room>();
        if (RoomManager.Instance != null) RoomManager.Instance._allRooms.Add(cloneRoom);

        var originalToClonePointsMap = new Dictionary<WallPoint, WallPoint>();
        var newWallPoints = new List<WallPoint>();

        foreach (WallPoint originalPoint in original._roomWallPoints)
        {
            GameObject wpGO = new GameObject($"WallPoint_Clone_{RoomManager.WallPointCountIndex++}");
            WallPoint newPoint = wpGO.AddComponent<WallPoint>();
            newPoint.Initialize(originalPoint._position);
            if (WallPointManager.Instance != null) WallPointManager.Instance._allWallPoints.Add(newPoint);
            originalToClonePointsMap[originalPoint] = newPoint;
            newWallPoints.Add(newPoint);
        }

        foreach (Wall originalWall in original._roomWalls)
        {
            GameObject wallGO = new GameObject($"Wall_Clone_{WallManager._wallIndex++}");
            Wall newWall = wallGO.AddComponent<Wall>();
            WallPoint newStartPoint = originalToClonePointsMap[originalWall.StartWallPoint];
            WallPoint newEndPoint = originalToClonePointsMap[originalWall.EndWallPoint];
            newWall.SetStartAndEndPosition(newStartPoint, newEndPoint);
            newStartPoint.AddConnectedWall(newWall);
            newEndPoint.AddConnectedWall(newWall);
            newStartPoint.AddConnectedWallPoint(newEndPoint);
            newEndPoint.AddConnectedWallPoint(newStartPoint);
            if (WallManager.Instance != null) WallManager.Instance._allWalls.Add(newWall);
        }

        cloneRoom.Initialize(newWallPoints);
        return cloneRoom;
    }*/

    private Room CreateDetachedClone(Room original)
    {
        GameObject roomGO = new GameObject("Room_Clone");
        Room cloneRoom = roomGO.AddComponent<Room>();

        // **CORRECT**: Register the new room with the RoomManager.
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance._allRooms.Add(cloneRoom);

            // **CORRECT**: Set the parent for a clean scene hierarchy.
            roomGO.transform.SetParent(RoomManager.Instance.transform);
        }

        var originalToClonePointsMap = new Dictionary<WallPoint, WallPoint>();
        var newWallPoints = new List<WallPoint>();

        // Step 1: Create and Register new WallPoints
        foreach (WallPoint originalPoint in original._roomWallPoints)
        {
            // ... (rest of the point creation logic is the same)
            GameObject wpGO = new GameObject($"WallPoint_Clone_{WallPointManager.WallPointCountIndex++}");
            WallPoint newPoint = wpGO.AddComponent<WallPoint>();
            newPoint.Initialize(originalPoint._position);

            if (WallPointManager.Instance != null)
            {
                WallPointManager.Instance._allWallPoints.Add(newPoint);
                wpGO.transform.SetParent(WallPointManager.Instance.transform); // Good practice to parent points too
            }

            originalToClonePointsMap[originalPoint] = newPoint;
            newWallPoints.Add(newPoint);
        }

        // Step 2: Create and Register new Walls
        foreach (Wall originalWall in original._roomWalls)
        {
            // ... (rest of the wall creation logic is the same)
            GameObject wallGO = new GameObject($"Wall_Clone_{WallManager.WallCountIndex++}");
            Wall newWall = wallGO.AddComponent<Wall>();

            if (WallManager.Instance != null)
            {
                WallManager.Instance._allWalls.Add(newWall);
                wallGO.transform.SetParent(WallManager.Instance.transform); // Good practice to parent walls too
            }

            WallPoint newStartPoint = originalToClonePointsMap[originalWall.StartWallPoint];
            WallPoint newEndPoint = originalToClonePointsMap[originalWall.EndWallPoint];

            newWall.SetStartAndEndPosition(newStartPoint, newEndPoint);
            newStartPoint.AddConnectedWall(newWall);
            newEndPoint.AddConnectedWall(newWall);
            newStartPoint.AddConnectedWallPoint(newEndPoint);
            newEndPoint.AddConnectedWallPoint(newStartPoint);
        }

        // Step 3: Initialize the clone room
        cloneRoom.Initialize(newWallPoints);
        return cloneRoom;
    }

    public void Update() 
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            RoomManager.Instance.DeleteRoom(ActiveRoom);
        }
    }
    public void SetActiveRoom(Room room) { }
    public void Init(Vector3 worldPos, Vector2 screenPos) { }
    public void OnPinch(float delta) { }
}