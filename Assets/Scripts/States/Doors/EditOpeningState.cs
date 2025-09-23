using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class EditOpeningState<T> : ICameraSubState where T : Opening
{
    private enum DragMode { None, Moving, ResizingLeft, ResizingRight }
    private DragMode _currentDragMode = DragMode.None;

    private T _selectedOpening;

    private T SelectedOpening
    {
        get { return _selectedOpening; }
        set
        {
            if (_selectedOpening == value) return;
            _selectedOpening = value;
            OpeningManager.Instance._currentSelectedOpening = value;
        }
    }

    private OrthoCam _orthoCam;
    private Vector3 _startPosition;

    private Vector3 _dragStartPosition;
    private float _startOpeningWidth;
    private Vector3 _startOpeningPosition;


    private EditUI _editUI;
    public EditOpeningState(OrthoCam orthoCam, T opening)
    {
        _orthoCam = orthoCam ?? GameManager.Instance.GetOrthoCamera();
        if (opening != null)
        {
            SelectedOpening = opening;
            SelectedOpening.OpeningVisual.SetHighlightedColor();
            SetEditUI();
        }
    }

    public void Enter()
    {
        Debug.Log($"Entered EditOpeningState<{typeof(T).Name}>");
    }

    public void Exit()
    {
        Debug.Log($"Exited EditOpeningState<{typeof(T).Name}>");
        if (SelectedOpening != null)
        {
            SelectedOpening.OpeningVisual.SetDefaultColor();
            SelectedOpening = null;
        }
        DestroyEditUI();
    }

    public void Update()
    {
        _orthoCam.Update();

        if (Input.GetKeyDown(KeyCode.G))
        {
            OpeningManager.Instance.DeleteOpening(_selectedOpening);
        }
    }
    public void Init(Vector3 worldPos, Vector2 screenPos) { }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        Ray ray = _orthoCam._mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            T opening = hit.collider.GetComponentInParent<T>();
            if (opening == null)
            {
                // Clicked on something else, so deselect
                if (SelectedOpening != null)
                {
                    SelectedOpening.OpeningVisual.SetDefaultColor();
                    SelectedOpening = null;
                }
                return;
            }

            // We hit an opening or its handle, select it if it's not already
            if (SelectedOpening != opening)
            {
                if (SelectedOpening != null) SelectedOpening.OpeningVisual.SetDefaultColor();
                SelectedOpening = opening;
                SelectedOpening.OpeningVisual.SetHighlightedColor();
                SetEditUI();
            }

            _dragStartPosition = worldPos;
            _startOpeningPosition = SelectedOpening.transform.position;
            _startOpeningWidth = SelectedOpening.Width;

            // Determine drag mode based on the collider's name
            switch (hit.collider.gameObject.name)
            {
                case "LeftHandle":
                    _currentDragMode = DragMode.ResizingLeft;
                    break;
                case "RightHandle":
                    _currentDragMode = DragMode.ResizingRight;
                    break;
                default:
                    _currentDragMode = DragMode.Moving;
                    _startPosition = SelectedOpening.OpeningPosition; // For move reversion
                    break;
            }
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedOpening == null) return;

        switch (_currentDragMode)
        {
            case DragMode.Moving:
                MoveOpening(_selectedOpening, worldPos);
                break;
            case DragMode.ResizingLeft:
            case DragMode.ResizingRight:
                ResizeOpening(worldPos);
                break;
        }
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (_selectedOpening == null) return;

        if (_currentDragMode == DragMode.Moving)
        {
            Wall nearest = FindNearestWall(worldPos, out Vector3 proj);
            proj.y = 3f;

            if (!AppHelper.CanPlaceOpening<T>(nearest, proj, _selectedOpening))
            {
                Debug.Log($"{typeof(T).Name} placement invalid, reverting.");
                MoveOpening(_selectedOpening, _startPosition);
            }
        }

        _currentDragMode = DragMode.None;
    }


    private void ResizeOpening(Vector3 currentWorldPos)
    {
        if (SelectedOpening == null || SelectedOpening.OpeningVisual._leftTransform == null || SelectedOpening.OpeningVisual._rightTransform == null) return;

        Vector3 widthDirection = (SelectedOpening.OpeningVisual._rightTransform.position - SelectedOpening.OpeningVisual._leftTransform.position).normalized;

        Wall nearestWall = FindNearestWall(currentWorldPos, out Vector3 projectedPos);
        if (nearestWall == null) return;

        Transform parentTransform = SelectedOpening.transform.parent;
        float scaleCorrection = 1.0f;
        if (parentTransform != null && parentTransform.lossyScale.x != 0)
        {
            scaleCorrection = 1.0f / parentTransform.lossyScale.x;
        }

        Vector3 dragVector = projectedPos - _dragStartPosition;
        float dragDelta = Vector3.Dot(dragVector, widthDirection) * scaleCorrection;


        float minWidth = AppHelper._doorWidth;

        if (_currentDragMode == DragMode.ResizingRight)
        {
            float maxNegativeDrag = minWidth - _startOpeningWidth;
            if (dragDelta < maxNegativeDrag)
            {
                dragDelta = maxNegativeDrag;
            }
        }
        else 
        {
            float maxPositiveDrag = _startOpeningWidth - minWidth;
            if (dragDelta > maxPositiveDrag)
            {
                dragDelta = maxPositiveDrag;
            }
        }

        float newWidth;
        float positionOffset;

        if (_currentDragMode == DragMode.ResizingRight)
        {
            newWidth = _startOpeningWidth + dragDelta;
            positionOffset = dragDelta / 2.0f;
        }
        else
        {
            newWidth = _startOpeningWidth - dragDelta;
            positionOffset = dragDelta / 2.0f;
        }

        SelectedOpening.Width = newWidth;
        SelectedOpening.transform.position = _startOpeningPosition + widthDirection * (positionOffset * scaleCorrection);
        SelectedOpening.OpeningPosition = SelectedOpening.ParentWall.transform.InverseTransformPoint(SelectedOpening.transform.position);
    }


    private T GetOpeningUnderTouch(Vector3 pos)
    {
        float minDist = float.MaxValue;
        T nearest = null;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            foreach (Opening opening in wall._allOpenings)
            {
                if (opening is T typedOpening)
                {
                    float dist = Vector3.Distance(pos, typedOpening.transform.position);
                    if (dist < minDist && dist < 5f)
                    {
                        minDist = dist;
                        nearest = typedOpening;
                    }
                }
            }
        }
        if (nearest != SelectedOpening && SelectedOpening != null)
        {
            SelectedOpening.OpeningVisual.SetDefaultColor();
        }
        return nearest;
    }

    private void MoveOpening(T opening, Vector3 worldPos)
    {
        Wall nearest = FindNearestWall(worldPos, out Vector3 proj);
        if (nearest == null) return;

        if (opening.ParentWall != nearest)
        {
            Wall oldWall = opening.ParentWall;
            if (oldWall != null) oldWall._allOpenings.Remove(opening);

            opening.transform.SetParent(nearest.transform, true);
            opening._parentWall = nearest;

            if (!nearest._allOpenings.Contains(opening))
                nearest._allOpenings.Add(opening);

            SetOpeningRotation(opening, nearest);
        }

        proj.y = opening.transform.position.y;
        opening.transform.position = proj;
        opening.OpeningPosition = nearest.transform.InverseTransformPoint(proj);
    }

    private Wall FindNearestWall(Vector3 point, out Vector3 closestPoint, float snapThreshold = 5f)
    {
        Wall nearest = null;
        float minDist = float.MaxValue;
        closestPoint = point;

        foreach (Wall wall in WallManager.Instance._allWalls)
        {
            if (wall == null) continue;

            Vector3 a = wall.GetStartPosition();
            Vector3 b = wall.GetEndPosition();
            GetClosestPoint(a, b, point, out Vector3 proj);

            float dist = Vector3.Distance(proj, point);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = wall;
                closestPoint = proj;
            }
        }

        if (minDist > snapThreshold) nearest = null;
        return nearest;
    }

    private void GetClosestPoint(Vector3 a, Vector3 b, Vector3 point, out Vector3 closest)
    {
        Vector3 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) { closest = a; return; }
        float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / len2);
        closest = a + ab * t;
    }

    private void SetOpeningRotation(T opening, Wall wall)
    {
        Vector3 dir = (wall.GetEndPosition() - wall.GetStartPosition()).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
        opening.OpeningVisual.transform.rotation = Quaternion.LookRotation(perp, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
    }

    public void OnPinch(float delta) => _orthoCam.ZoomCamera(delta);


    private void SetEditUI()
    {
        if (SelectedOpening == null) return;

        Vector3 yOffset = Vector3.up * 1f;
        Vector3 zOffset = Vector3.forward * 3f;

        Vector3 position = SelectedOpening.OpeningPosition + yOffset + zOffset;

        if (_editUI == null)
        {
            // Instantiate and parent under the wall point
            _editUI = GameObject.Instantiate(
                GameManager.Instance._uiManager._editUIPrefab,
                position,
                Quaternion.identity,
                SelectedOpening.transform
            );
            _editUI.gameObject.name = "EditUI";
        }
        else
        {
            _editUI.transform.SetParent(SelectedOpening.transform, false);
            _editUI.transform.position = position;
        }

        _editUI.Initialize(EditUIType.OpeningEdit);
    }

    private void DestroyEditUI()
    {
        if (_editUI != null)
        {
            GameObject.Destroy(_editUI.gameObject);
        }
    }
}