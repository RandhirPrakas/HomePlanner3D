using System.Collections.Generic;
using UnityEngine;

public class EditRoomPointsState : ICameraSubState
{
    private WallPoint _selectedPoint;
    private GameObject _highlightParent;

    private readonly Color _highlightedColor = new Color(136, 91, 255, 255);
    public void Enter()
    {
        Debug.Log("Entered EditRoomPointsState");

        _highlightParent = new GameObject("WallPointHighlights");

        int i = 0;
        foreach (WallPoint point in WallPointManager.Instance._allWallPoints)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "sphere" + i;
            sphere.transform.position = point._position;
            sphere.transform.localScale = Vector3.one * 1.5f;
            sphere.GetComponent<Renderer>().material.color = Color.yellow;
            sphere.transform.SetParent(_highlightParent.transform);

            // Link sphere to wall point
            point.SetHighlightVisual(sphere);
        }
    }

    public void Exit()
    {
        Debug.Log("Exited EditRoomPointsState");

        // Clean up all highlight visuals
        if (_highlightParent != null)
            GameObject.Destroy(_highlightParent);
    }

    public void Update() { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _selectedPoint = GetPointUnderTouch(worldPos);
        _selectedPoint._activeSphere.GetComponent<MeshRenderer>().material.color = _highlightedColor;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedPoint != null)
        {
            _selectedPoint.SetPosition(worldPos + Vector3.up * AppHelper._lrYPos);
        }

        if (_selectedPoint != null)
        {
            var allOtherPoints = WallPointManager.Instance._allWallPoints
                .FindAll(p => p != _selectedPoint);

            Vector3 snappedPosition = AppHelper.SmartSnapToAxis(worldPos, allOtherPoints);

           
            snappedPosition += Vector3.up * AppHelper._lrYPos;

            
            _selectedPoint.SetPosition(snappedPosition);
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedPoint != null)
        {
            Vector3 snappedPos = AppHelper.SmartSnapToAxis(worldPos, WallPointManager.Instance._allWallPoints);
            snappedPos += Vector3.up * AppHelper._lrYPos;

            WallPoint target = WallPointManager.Instance.GetExistingPointAt(snappedPos, _selectedPoint);


            if (target != null)
            {
                _selectedPoint.MergeWith(target);
                HashSet<Room> affectedRooms = target.GetParentRooms();

                foreach (Room room in affectedRooms)
                {
                    //room._allRoomWalls.RemoveAll(w => w == null);
                    room.CleanUpNullWalls();

                    foreach (var wall in room._allRoomWalls)
                    {
                        wall.UpdateFromPoints();
                    }

                    room.UpdateFloorOnEditingPoints();
                }
            }
            else
            {
                _selectedPoint.SetPosition(snappedPos);
            }

            _selectedPoint._activeSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
            _selectedPoint = null;
        }


    }


    private WallPoint GetPointUnderTouch(Vector3 position)
    {
        foreach (WallPoint point in WallPointManager.Instance._allWallPoints)
        {
            if (Vector3.Distance(position + Vector3.up * AppHelper._lrYPos, point._position) < 10f) 
            {
                return point;
            }
        }
        return null;
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }
}
