using TMPro.EditorUtilities;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class DrawRoomState : ICameraSubState
{
    private Vector3 _startPos;
    private Room _currentRoom;
    private ProceduarlwallGenerator _wallGenerator;
    private LineRenderer _wallOutline;

    private bool _foundNearestPoint = false;


    public void Enter()
    {
        Debug.Log("Entered DrawRoomState");

        // Create new Room GameObject
        GameObject roomGO = new GameObject("Room");
        _currentRoom = roomGO.AddComponent<Room>();

        _currentRoom.SpawnWallLabelCanvas();

        _wallOutline = roomGO.AddComponent<LineRenderer>();
        _wallOutline.positionCount = 0;
        _wallOutline.material = Resources.Load<Material>("ProceduralMaterials/QuadMaterial 1");
        _wallOutline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _wallGenerator = new ProceduarlwallGenerator();
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
            if (AppHelper.CanSnapPoint(position, wp._position))
            {
                _startPos = AppHelper.SnapPoint(wp._position, position);
                _foundNearestPoint = true;
                break;
            }
        }
        if (!_foundNearestPoint)
        {
            _startPos = position;
            _foundNearestPoint = false;
        }
        _wallOutline.positionCount = 1;
        _wallOutline.SetPosition(0, _startPos + Vector3.up * 10);
    }

    public void OnTouchHold(Vector3 position)
    {
        if (_wallOutline.positionCount != 2)
        {
            _wallOutline.positionCount = 2;
            _wallOutline.SetPosition(0, _startPos + Vector3.up * 10);
        }

        _wallOutline.SetPosition(1, position + Vector3.up * 10);
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
        // Create a parent GameObject to hold all 6 quads
        GameObject wallGO = new GameObject("Wall");
        wallGO.transform.parent = _currentRoom.transform;

        //Create Wall
        Wall wallComp = wallGO.AddComponent<Wall>();

        // Create its wall Point
        WallPoint startWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(_startPos, "StartWallPoint");

        WallPoint endWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(position, "EndWallPoint");

        startWallPoint.transform.SetParent(wallComp.transform);
        endWallPoint.transform.SetParent(wallComp.transform);

        // Attach Wall Point to the wall
        wallComp.SetStartAndEndPosition(startWallPoint, endWallPoint);

        // Generate procedural wall geometry under this wall GameObject
        //_wallGenerator.MapAllRequiredPoints(startWallPoint._position, endWallPoint._position, wallGO.transform);

        ResetWallOutlineBase();
        // Register wall in Room
        _currentRoom._allRoomWalls.Add(wallComp);
    }

    private void ResetWallOutlineBase()
    {
        _wallOutline.positionCount = 0;
    }

}
