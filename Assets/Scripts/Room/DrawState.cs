using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DrawState : ICameraSubState
{
    private Vector3 _startPos;
    private Vector3 _snappedEnd;

    private Grid _grid;

    private Transform _strandedWalls;

    private Wall _firstNearestWall, _secondNearestWall;
    public DrawState()
    {
        Debug.Log("Draw State Initialized");
        _strandedWalls = GameObject.Find("StrandedWalls").transform;
    }
    public void Enter()
    {
        Debug.Log("Entered Draw State ");
        if (_grid == null)
            _grid = GameObject.FindObjectOfType<Grid>();

        _firstNearestWall = null;
        _secondNearestWall = null;
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
        if (Vector3.Distance(_startPos, worldPos) < AppHelper._minimumWallLength)
        {
            Debug.Log("Not Enough Points");
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

    }

    private void DrawSingleWall(Vector3 endPosition)
    {

        GameObject wallGO = new GameObject($"Wall_{WallManager._wallIndex++}");
        Wall wallComp = wallGO.AddComponent<Wall>();
        wallGO.transform.SetParent(_strandedWalls);

        // Create/Get wall points
        WallPoint startWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(_startPos);

        endPosition = AppHelper.SmartSnapToAxis(endPosition, WallPointManager.Instance._allWallPoints);
        endPosition = AppHelper.WrapPosition(_startPos, endPosition);

        WallPoint endWallPoint = WallPointManager.Instance.CreateOrGetwallPoints(endPosition + Vector3.up * AppHelper._lrYPos);

        startWallPoint.transform.SetParent(WallPointManager.Instance.transform, true);
        endWallPoint.transform.SetParent(WallPointManager.Instance.transform, true);

        // Add Connected Wall Points
        startWallPoint.AddConnectedWallPoint(endWallPoint);
        endWallPoint.AddConnectedWallPoint(startWallPoint);

        // Add Connected Walls
        startWallPoint.AddConnectedWall(wallComp);
        endWallPoint.AddConnectedWall(wallComp);

        // AddRequired WallPoints
        AddAdditionalWallPoint(startWallPoint, _firstNearestWall);
        AddAdditionalWallPoint(endWallPoint, _secondNearestWall);

        // Add the Current Wallpoint
        AddCurrentWallpoint(_firstNearestWall, startWallPoint);
        AddCurrentWallpoint(_secondNearestWall, endWallPoint);

        // RemoveRedundant wallPoints
        SplitConnectedWall(_firstNearestWall, startWallPoint);
        SplitConnectedWall(_secondNearestWall, endWallPoint);

        wallComp.SetStartAndEndPosition(startWallPoint, endWallPoint);

        WallManager.Instance._allWalls.Add(wallComp);
        AppEventHandler.InvokeOnWallCreation();
    }

    private void AddAdditionalWallPoint(WallPoint wallPoint, Wall wall = null)
    {
        if(wall != null)
        {
            wallPoint.AddConnectedWallPoint(wall.GetStartWallPoint());
            wallPoint.AddConnectedWallPoint(wall.GetEndWallPoint());
        }
    }

   
    public bool TrySnapToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd, out Vector3 snappedPoint)
    {
        snappedPoint = point;

        if (Vector3.Distance(point, lineEnd) <= 1 || Vector3.Distance(point, lineStart) <= 1)
        {
            Debug.Log("Point is Too Close to the end");
            return false;
        }

        if (lineStart == lineEnd)
            return false;

        Vector3 lineDir = lineEnd - lineStart;
        float t = Vector3.Dot(point - lineStart, lineDir) / lineDir.sqrMagnitude;
        t = Mathf.Clamp01(t);

        Vector3 closest = lineStart + t * lineDir;
        float dist = Vector3.Distance(point, closest);

        if (dist <= AppHelper._nearestWallSnapThreshold)
        {
            snappedPoint = closest;
            return true;
        }

        return false;
    }

    public Vector3 TryGetNearestWall(Vector3 snappedPosition, bool isFirstTouch)
    {
        Debug.Log("Trying to get the wall ");
        Vector3 newSnappedPosition = Vector3.zero;
        foreach(Wall wall in WallManager.Instance._allWalls)
        {
            bool nearWall = TrySnapToLine(snappedPosition, wall.GetStartPosition(), wall.GetEndPosition(), out newSnappedPosition);
            if(nearWall)
            {
                if(isFirstTouch)
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

    private void SplitConnectedWall(Wall wall, WallPoint splitPoint)
    {
        if (wall == null)
            return;
        DrawWall(wall.GetStartWallPoint(), splitPoint);
        DrawWall(splitPoint, wall.GetEndWallPoint());

        wall.GetStartWallPoint().RemoveConnectedWallPoint(wall.GetEndWallPoint());
        wall.GetEndWallPoint().RemoveConnectedWallPoint(wall.GetStartWallPoint());
        wall.DeleteWall();
    }

    private void AddCurrentWallpoint(Wall wall, WallPoint currentWallpoint)
    {
        if (wall == null || currentWallpoint == null)
            return;
        wall.GetStartWallPoint().AddConnectedWallPoint(currentWallpoint);
        wall.GetEndWallPoint().AddConnectedWallPoint(currentWallpoint);
    }

    private void ManageWallsAndWallPoints(Vector3 start, Vector3 end)
    {
        Vector3 intersectionPoint = Vector3.zero;
        foreach(Wall wall in WallManager.Instance._allWalls)
        {

            if(AppHelper.IsPointOnLineSegment(wall.GetStartPosition(), wall.GetEndPosition(),end))
            {
                // Draw wall F
                return;
            }
            else if(AppHelper.TryGetLineIntersection(start, end, wall.GetStartPosition(), wall.GetEndPosition(),out intersectionPoint))
            {

            }
            else
            {
                // Draw Normal Wall
            }
        }
        // else check for each wall if it intersect

        // else just draw wall
    }

    private void DrawWall(WallPoint startPoint, WallPoint endPoint)
    {
        GameObject wallGO = new GameObject($"Wall_{WallManager._wallIndex++}");
        Wall wallComp = wallGO.AddComponent<Wall>();
        wallGO.transform.SetParent(_strandedWalls);
        wallComp.SetStartAndEndPosition(startPoint, endPoint);

        WallManager.Instance._allWalls.Add(wallComp);
    }
    

}
