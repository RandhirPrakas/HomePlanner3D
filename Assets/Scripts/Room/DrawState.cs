using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawState : ICameraSubState
{
    private OrthoCam _orthoCam;
    private Vector3 _startPos;
    private Vector3 _snappedEnd;

    private Grid _grid;
    private Transform _strandedWalls;
    private Wall _firstNearestWall, _secondNearestWall;

    // --- Preview Visuals ---
    private GameObject _previewObject;
    private LineRenderer _previewLine;
    private TextMeshPro _previewLengthText;
    private Material _lrMaterial;
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

        GameManager.Instance._uiManager.OnExitOrthoIdleState();
        _strandedWalls = GameObject.Find("StrandedWalls").transform;

        _lrMaterial = Resources.Load<Material>("ProceduralMaterials/DefaultLRmaterial");
    }

    public void Exit()
    {
        Debug.Log("Exiting Draw State");
        //Ensure preview is destroyed if state is exited unexpectedly
        DestroyPreview();
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
                    _startPos.y = 0.1f;
                    CreatePreview(_startPos);
                    return;
                }
            }

            if (!snappedToPoint)
                _startPos = _grid.GetNearestPointOnGrid(worldPos);
        }

        _startPos = TryGetNearestWall(_startPos, true);
        _startPos.y = AppHelper._lrYPos;

        // Create the visual preview for drawing
        CreatePreview(_startPos);
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        worldPos.y = AppHelper._lrYPos;

        // Update the visual preview
        UpdatePreview(worldPos);
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        // Drawing is finished, destroy the visuals - LR and Text
        DestroyPreview();

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
            _snappedEnd = AppHelper.SmartSnapToAxis(_snappedEnd, WallPointManager.Instance._allWallPoints); 
            _snappedEnd = AppHelper.WrapPosition(_startPos, _snappedEnd);
        }

        _snappedEnd = TryGetNearestWall(_snappedEnd, false);
        _snappedEnd.y = AppHelper._lrYPos;

        DrawSingleWall(_snappedEnd);

        // cleanup

        // sometimes _startPosition is no updated if we start drawing next wall very quickly, so I am deliberately setting the start position to the end of the wall
        _startPos = _snappedEnd;

    }

    public void Update()
    {
        _orthoCam.Update();
    }

    private void DrawSingleWall(Vector3 endPosition)
    {
        endPosition.y = 0;
        _startPos.y = 0;
        AppHelper.ManageWallsAndWallPoints(_startPos, endPosition, _strandedWalls);
        AppEventHandler.InvokeOnWallCreation();
    }

    public Vector3 TryGetNearestWall(Vector3 snappedPosition, bool isFirstTouch)
    {
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

    #region Preview Methods

    /// <summary>
    /// Creates the temporary LineRenderer and TextMeshPro for visual feedback during drawing.
    /// </summary>
    private void CreatePreview(Vector3 startPos)
    {
        DestroyPreview();
        _previewObject = new GameObject("WallDrawPreview");

        // --- Setup LineRenderer ---
        _previewLine = _previewObject.AddComponent<LineRenderer>();
        _previewLine.positionCount = 2;
        startPos.y = 0.6f;
        _previewLine.SetPosition(0, startPos);
        _previewLine.SetPosition(1, startPos);

        // Style the line
        _previewLine.startWidth = 0.5f;
        _previewLine.endWidth = 0.5f;
        _previewLine.material = _lrMaterial;
        Color lineColor = new Color(0.2f, 0.5f, 1f, 0.7f);
        _previewLine.startColor = lineColor;
        _previewLine.endColor = lineColor;
        _previewLine.useWorldSpace = true;

        // --- Setup TextMeshPro ---
        GameObject textObject = new GameObject("PreviewLengthText");
        textObject.transform.SetParent(_previewObject.transform);
        _previewLengthText = textObject.AddComponent<TextMeshPro>();

        // Style the text
        _previewLengthText.fontSize = 10f;
        _previewLengthText.alignment = TextAlignmentOptions.Center;
        _previewLengthText.color = Color.white;
        _previewLengthText.text = "0.00 ft";
        _previewLengthText.transform.rotation = Quaternion.Euler(90f, 0, 0);
    }

    /// <summary>
    /// Updates the preview line and text to follow the user's touch/cursor position.
    /// </summary>
    private void UpdatePreview(Vector3 currentPos)
    {
        if (_previewObject == null) return;

        // Apply the same snapping rules to the preview for accuracy
        Vector3 snappedCurrentPos = AppHelper.SmartSnapToAxis(currentPos, WallPointManager.Instance._allWallPoints);
        snappedCurrentPos = AppHelper.WrapPosition(_startPos, snappedCurrentPos);
        snappedCurrentPos.y = 0.6f;

        // Update line renderer's end point
        _previewLine.SetPosition(1, snappedCurrentPos);

        // Update length text
        float length = Vector3.Distance(_startPos, snappedCurrentPos);
        _previewLengthText.text = $"{length:F2} ft";

        // --- Update text position and rotation with perpendicular offset ---
        Vector3 center = (_startPos + snappedCurrentPos) * 0.5f;
        Vector3 dir = (snappedCurrentPos - _startPos).normalized;

        // Get the perpendicular direction on the XZ plane
        Vector3 perpendicular = new Vector3(-dir.z, 0, dir.x);

        float offsetDistance = 0.75f; // How far the label should be from the wall
        // Position the label at the center, then move it out by the perpendicular offset
        _previewLengthText.transform.position = center + (perpendicular * offsetDistance);


        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            _previewLengthText.transform.rotation = Quaternion.Euler(90f, -angle, 0f);
        }
    }

    /// <summary>
    /// Destroys the preview GameObject and cleans up references.
    /// </summary>G
    private void DestroyPreview()
    {
        if (_previewObject != null)
        {
            GameObject.Destroy(_previewObject);
            _previewObject = null;
            _previewLine = null;
            _previewLengthText = null;
        }
    }

    #endregion
}

