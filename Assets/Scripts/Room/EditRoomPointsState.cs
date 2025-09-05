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
        if(_selectedPoint != null)
            _selectedPoint._activeSphere.GetComponent<MeshRenderer>().material.color = _highlightedColor;
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedPoint != null)
        {
            _selectedPoint.SetPosition(worldPos + Vector3.up * AppHelper._lrYPos);

            var allOtherPoints = WallPointManager.Instance._allWallPoints
                .FindAll(p => p != _selectedPoint);

            Vector3 snappedPosition = AppHelper.SmartSnapToAxis(worldPos, allOtherPoints);


            snappedPosition += Vector3.up * AppHelper._lrYPos;

            _selectedPoint.SetPosition(snappedPosition);

            // To Move the wall accordingly
            // Issue occurs after merging
            foreach (Wall wall in _selectedPoint.GetConnectedWalls())
            {
                wall.UpdateFromPoints(true);
            }
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedPoint == null) return;

        Vector3 snappedPos = AppHelper.SmartSnapToAxis(worldPos, WallPointManager.Instance._allWallPoints);
        snappedPos += Vector3.up * AppHelper._lrYPos;

        WallPoint targetPointToMerge = WallPointManager.Instance.GetExistingPointAt(snappedPos, _selectedPoint);
        if (targetPointToMerge != null)
        {
            Debug.Log("Action: Merging with existing point.");
            _selectedPoint.MergeWith(targetPointToMerge);

            _selectedPoint = null;
            return;
        }

        var wallsToRedraw = new List<KeyValuePair<Wall, WallPoint>>();
        foreach (Wall wall in _selectedPoint.GetConnectedWalls())
        {
            WallPoint otherPoint = wall.GetStartWallPoint() == _selectedPoint ? wall.GetEndWallPoint() : wall.GetStartWallPoint();
            wallsToRedraw.Add(new KeyValuePair<Wall, WallPoint>(wall, otherPoint));
        }

        foreach (var pair in wallsToRedraw)
        {
            Wall wall = pair.Key;
            WallManager.Instance.DestroyWall(wall);
        }

        _selectedPoint.SetPosition(snappedPos);

        foreach (var pair in wallsToRedraw)
        {
            WallPoint otherPoint = pair.Value;
            Debug.Log($"Redrawing wall from {otherPoint._position} to {_selectedPoint._position}");
            AppHelper.ManageWallsAndWallPoints(otherPoint._position, _selectedPoint._position);
        }

        if (_selectedPoint != null && _selectedPoint._activeSphere != null)
        {
            _selectedPoint._activeSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        _selectedPoint = null;

        AppEventHandler.InvokeOnWallCreation();
    }

    private WallPoint GetPointUnderTouch(Vector3 position)
    {
        WallPoint closestPoint = null;
        float closestSqrDist = float.MaxValue;

        Vector3 adjustedPos = position + Vector3.up * AppHelper._lrYPos;
        float thresholdSqr = AppHelper._pointSnapThreshold * AppHelper._pointSnapThreshold;

        foreach (WallPoint point in WallPointManager.Instance._allWallPoints)
        {
            float sqrDist = (adjustedPos - point._position).sqrMagnitude;

            if (sqrDist < thresholdSqr && sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    public void Init(Vector3 worldPos, Vector2 screenPos)
    {
        throw new System.NotImplementedException();
    }

}
