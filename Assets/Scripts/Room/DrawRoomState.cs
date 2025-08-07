using System.Collections.Generic;
using UnityEngine;

public class DrawRoomState : ICameraSubState
{
    private Vector3 _startPos;
    private Room _currentRoom;
    private ProceduarlwallGenerator _wallGenerator;
    private LineRenderer _wallOutline;

    private bool _foundNearestPoint = false;

    public DrawRoomState(Room existingRoom = null)
    {
        if (existingRoom != null)
        {
            _currentRoom = existingRoom;
        }
        else
        {
            _currentRoom = new GameObject("Room").AddComponent<Room>();
            RoomManager.Instance._allRooms.Add(_currentRoom);
        }
    }


    public void Enter()
    {
        Debug.Log("Entered DrawRoomState");

        if (_currentRoom == null)
        {

            // Create new Room GameObject
            GameObject roomGO = new GameObject("Room");
            _currentRoom = roomGO.AddComponent<Room>();
            RoomManager.Instance._allRooms.Add(_currentRoom);

        }
            _currentRoom.SpawnWallLabelCanvas();

        if (_wallOutline == null)
        {
            _wallOutline = _currentRoom.gameObject.AddComponent<LineRenderer>();
            _wallOutline.positionCount = 0;
            _wallOutline.material = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial");
            _wallOutline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _wallOutline.startWidth = AppHelper._lrThickness;
            _wallOutline.endWidth = AppHelper._lrThickness;
            _wallGenerator = new ProceduarlwallGenerator();
        }
    }

    public void Exit()
    {
        Debug.Log("Exited DrawRoomState");
        _currentRoom = null;
    }

    public void Update()
    {
        // Optional: Add real-time preview
    }

    public void OnTouchStart(Vector3 position)
    {
        foreach (WallPoint wp in WallPointManager.Instance._allWallPoints)
        {
            if (AppHelper.CanSnapPoint(position + Vector3.up * AppHelper._lrYPos, wp._position))
            {
                _startPos = wp._position;
                _foundNearestPoint = true;
                break;
            }
        }
        if (!_foundNearestPoint)
        {
            _startPos = position + Vector3.up * AppHelper._lrYPos;
            _foundNearestPoint = false;
        }
        _wallOutline.positionCount = 1;
        _wallOutline.SetPosition(0, _startPos);
    }

    /*public void OnTouchHold(Vector3 position)
    {
        if (_wallOutline.positionCount != 2)
        {
            _wallOutline.positionCount = 2;
            _wallOutline.SetPosition(0, _startPos);
        }
        position = AppHelper.WrapPosition(_startPos, position);
        _wallOutline.SetPosition(1, position + Vector3.up * AppHelper._lrYPos);
    }*/


    public void OnTouchHold(Vector3 position)
    {
        if (_wallOutline.positionCount != 2)
        {
            _wallOutline.positionCount = 2;
            _wallOutline.SetPosition(0, _startPos);
        }

        // Get wall points and apply smart snapping
        position = AppHelper.SmartSnapToAxis(position, WallPointManager.Instance._allWallPoints);

        // Apply wrapping (maintain orthogonal)
        position = AppHelper.WrapPosition(_startPos, position);

        _wallOutline.SetPosition(1, position + Vector3.up * AppHelper._lrYPos);
    }

    public void OnTouchEnd(Vector3 position)
    {
        if (Vector3.Distance(_startPos, position) < AppHelper._minimumWallLength)
        {
            Debug.Log("Not Enough Points");
            _wallOutline.positionCount -= 1;
            return;
        }

        DrawSingleWall(position);
    }
    private void DrawSingleWall(Vector3 position)
    {
        GameObject wallGO = new GameObject("Wall");
        wallGO.transform.parent = _currentRoom.transform;

        //Create Wall
        Wall wallComp = wallGO.AddComponent<Wall>();

        // Create its wall Point
        WallPoint startWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(_startPos, "StartWallPoint");

        position = AppHelper.SmartSnapToAxis(position, WallPointManager.Instance._allWallPoints);
        position = AppHelper.WrapPosition(_startPos, position);
        WallPoint endWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(position + Vector3.up * AppHelper._lrYPos, "EndWallPoint");

        startWallPoint.transform.SetParent(wallComp.transform);
        endWallPoint.transform.SetParent(wallComp.transform);

        startWallPoint._connectedWalls.Add(wallComp);
        endWallPoint._connectedWalls.Add(wallComp);

        // Attach Wall Point to the wall
        wallComp.SetStartAndEndPosition(startWallPoint, endWallPoint, _currentRoom);


        ResetWallOutlineBase();

        // Add this generate wall to current room 
        _currentRoom._allRoomWalls.Add(wallComp);

        Debug.Log($"Start Position = {_startPos}");
        Debug.Log($"End Position = {position + Vector3.up * AppHelper._lrYPos}");
        _currentRoom._wallCorners.Add(startWallPoint._position);
        _currentRoom._wallCorners.Add(endWallPoint._position);

        AppHelper.InvokeOnWallCreation();
    }

    private void ResetWallOutlineBase()
    {
        _wallOutline.positionCount = 0;
        _foundNearestPoint = false;
    }


    public void StartNewRoom()
    {
        GameObject roomGO = new GameObject("Room");
        Room newRoom = roomGO.AddComponent<Room>();
        newRoom.SpawnWallLabelCanvas();

        _currentRoom = newRoom;

    }
}
