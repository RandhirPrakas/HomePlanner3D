using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class EditRoomPointsState : ICameraSubState
{
    private WallPoint _selectedPoint;
    private GameObject _highlightParent;

    public void Enter()
    {
        Debug.Log("Entered EditRoomPointsState");

        _highlightParent = new GameObject("WallPointHighlights");

        foreach (WallPoint point in WallPointManager.Instance._allWallPoints)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point._position;
            sphere.transform.localScale = Vector3.one * 3f;
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

    public void OnTouchStart(Vector3 position)
    {
        _selectedPoint = GetPointUnderTouch(position);
    }

    public void OnTouchHold(Vector3 position)
    {
        if (_selectedPoint != null)
        {
            _selectedPoint.SetPosition(position + Vector3.up * AppHelper._lrYPos);
        }

        if (_selectedPoint != null)
        {
            var allOtherPoints = WallPointManager.Instance._allWallPoints
                .FindAll(p => p != _selectedPoint);

            Vector3 snappedPosition = AppHelper.SmartSnapToAxis(position, allOtherPoints);

           
            snappedPosition += Vector3.up * AppHelper._lrYPos;

            
            _selectedPoint.SetPosition(snappedPosition);
        }
    }

    public void OnTouchEnd(Vector3 position)
    {
        if (_selectedPoint != null)
        {
            Vector3 snappedPos = AppHelper.SmartSnapToAxis(position, WallPointManager.Instance._allWallPoints);
            snappedPos += Vector3.up * AppHelper._lrYPos;

            WallPoint target = WallPointManager.Instance.GetExistingPointAt(snappedPos, _selectedPoint);

            /*if (target != null)
            {
                _selectedPoint.MergeWith(target);

                _selectedPoint = target;

                HashSet<Room> affectedRooms = _selectedPoint.GetParentRooms();

                foreach (Room room in affectedRooms)
                {
                    room.CleanUpNullWalls();
                    room.UpdateFloorOnEditingPoints();
                }
            }*/

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
        
}
