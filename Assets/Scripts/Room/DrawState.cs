using System.Collections.Generic;
using UnityEngine;

public class DrawState : ICameraSubState
{
    private OrthoCam _orthoCam;
    private Vector3 _startPos;
    private Vector3 _snappedEnd;

    private Grid _grid;

    private Transform _strandedWalls;

    private Wall _firstNearestWall, _secondNearestWall;
    public DrawState(OrthoCam orthocam)
    {
        Debug.Log("Draw State Initialized");

        _orthoCam = orthocam;
        
    }

    public void Enter()
    {
        Debug.Log("Entered Draw State ");
        if (_grid == null)
            _grid = GameObject.FindObjectOfType<Grid>();

        _firstNearestWall = null;
        _secondNearestWall = null;

        GameManager.Instance._uiManager.SetDrawButtonActive(false);
        _strandedWalls = GameObject.Find("StrandedWalls").transform;
    }

    public void Exit()
    {
        Debug.Log("Exiting Draw State");
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = 0.1f;
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
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

        _startPos = TryGetNearestWall(_startPos, true);
        _startPos.y = AppHelper._lrYPos;

    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = 0.1f;
        worldPos = AppHelper.SmartSnapToAxis(worldPos, WallPointManager.Instance._allWallPoints);
        worldPos = AppHelper.WrapPosition(_startPos, worldPos);

    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_startPos == Vector3.zero)
        {
            Debug.LogWarning("OnTouchEnd called but startPos not set!");
            return;
        }

        if (Vector3.Distance(_startPos, worldPos) < AppHelper._minimumWallLength)
        {
            Debug.Log("Not Enough Distance");
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

        _snappedEnd = TryGetNearestWall(_snappedEnd, false);
        _snappedEnd.y = AppHelper._lrYPos;


        DrawSingleWall(_snappedEnd);
    }


    public void Update()
    {
        _orthoCam.Update();
    }

    private void DrawSingleWall(Vector3 endPosition)
    {
        endPosition = AppHelper.SmartSnapToAxis(endPosition, WallPointManager.Instance._allWallPoints);
        endPosition = AppHelper.WrapPosition(_startPos, endPosition);
        endPosition.y = 0;
        AppHelper.ManageWallsAndWallPoints(_startPos, endPosition, _strandedWalls);

        AppEventHandler.InvokeOnWallCreation();

    }


    public Vector3 TryGetNearestWall(Vector3 snappedPosition, bool isFirstTouch)
    {
        Debug.Log("Trying to get the wall ");
        Vector3 newSnappedPosition = Vector3.zero;
        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            bool nearWall = AppHelper.TrySnapToLine(snappedPosition, wall.GetStartPosition(), wall.GetEndPosition(), out newSnappedPosition);
            if (nearWall)
            {
                if (isFirstTouch)
                {
                    _firstNearestWall = wall;
                }
                else
                {
                    _secondNearestWall = wall;
                }
                return newSnappedPosition;
            }
        }

        return snappedPosition;
    }

    public void OnPinch(float delta)
    {
        _orthoCam.ZoomCamera(delta);
    }
}
