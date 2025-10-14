using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditOpeningIn3DState<T> : ICameraSubState where T : Opening
{
    private T _selectedOpening;
    private Camera _camera;

    [SerializeField] private LayerMask _wallLayerMask;

    private GameObject _resizeHandlePrefab;
    private List<ResizeHandle> _resizeHandles = new List<ResizeHandle>();
    private ResizeHandle _currentSelectedHandle;
    private Vector2 _lastFrameScreenPos;

    public T SelectedOpening
    {
        get => _selectedOpening;
        set
        {
            if (_selectedOpening == value) return;
            _selectedOpening = value;
        }
    }

    public EditOpeningIn3DState(Camera camera, T Opening)
    {
        _camera = camera ?? Camera.main;
        _wallLayerMask = LayerMask.GetMask(Constants.LAYER_WALL);
        SelectedOpening = Opening;
        ShowResizer(Opening);
    }

    public void Enter() { }
    public void Init(Vector3 worldPos, Vector2 screenPos) { }
    public void Update() { }

    public void Exit()
    {
        Debug.Log($"Exited EditOpeningIn3DState<{typeof(T).Name}>");
        foreach (ResizeHandle handle in _resizeHandles)
        {
            if (handle != null) { GameObject.Destroy(handle.gameObject); }
        }
        _resizeHandles.Clear();
        _selectedOpening = null;
    }

    public void OnTouchStart(Vector3 worldPos, Vector2 screenPos)
    {
        _lastFrameScreenPos = screenPos;
        Ray ray = _camera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (hit.collider.TryGetComponent<ResizeHandle>(out var resizeHandle))
            {
                _currentSelectedHandle = resizeHandle;
                SelectedOpening = resizeHandle.ownerOpening as T;
            }
            else if (hit.collider.TryGetComponent<T>(out var opening))
            {
                SelectedOpening = opening;
                _currentSelectedHandle = null;
            }
        }
    }

    public void OnTouchHold(Vector3 worldPos, Vector2 screenPos)
    {
        if (SelectedOpening == null) return;

        if (_currentSelectedHandle != null)
        {
            ResizeOpening(screenPos);
        }
        else
        {
            MoveOpeningOnWall(screenPos, false);
        }
        _lastFrameScreenPos = screenPos;
    }

    public void OnTouchEnd(Vector3 worldPos, Vector2 screenPos)
    {
        if (SelectedOpening != null && _currentSelectedHandle == null)
        {
            ClearWallSegment();
            MoveOpeningOnWall(screenPos, true);
        }
    }

    public void OnPinch(float delta)
    {
        if (_camera.orthographic) { _camera.orthographicSize = Mathf.Max(0.1f, _camera.orthographicSize - delta); }
        else { _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - delta, 20f, 80f); }
    }

    #region Helpers

    private void MoveOpeningOnWall(Vector2 screenPos, bool createCol = true)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _wallLayerMask))
        {
            if (!hit.collider.TryGetComponent<Wall>(out Wall nearestWall))
            {
                nearestWall = hit.collider.GetComponentInParent<Wall>();
            }

            if (nearestWall != null)
            {
                Vector3 wallStart = nearestWall.GetStartPosition();
                Vector3 wallEnd = nearestWall.GetEndPosition();
                Vector3 wallLine = wallEnd - wallStart;
                Vector3 projectedVector = Vector3.Project(hit.point - wallStart, wallLine.normalized);
                float t = projectedVector.magnitude / wallLine.magnitude;
                Vector3 targetPos = Vector3.Lerp(wallStart, wallEnd, Mathf.Clamp01(t));

                if (SelectedOpening is Window) { targetPos.y = hit.point.y; }
                else { targetPos.y = SelectedOpening.transform.position.y; }

                if (SelectedOpening.ParentWall != nearestWall)
                {
                    // Logic for switching walls
                    Wall oldWall = SelectedOpening.ParentWall;
                    SelectedOpening._lastWall = oldWall;
                    if (oldWall != null) oldWall._allOpenings.Remove(SelectedOpening);
                    SelectedOpening.transform.SetParent(nearestWall.transform, true);
                    SelectedOpening.ParentWall = nearestWall;
                    if (!nearestWall._allOpenings.Contains(SelectedOpening)) nearestWall._allOpenings.Add(SelectedOpening);
                }

                SelectedOpening.transform.position = targetPos;
                SelectedOpening.transform.rotation = Quaternion.LookRotation((nearestWall.GetEndPosition() - nearestWall.GetStartPosition()).normalized, Vector3.up);
                SelectedOpening.OpeningPosition = nearestWall.transform.InverseTransformPoint(targetPos);
                SelectedOpening.CalculateAndSetNormalizedPosition(targetPos);

                WallMeshGenerator.GenerateWallWithOpenings(nearestWall, createCol);

                if (SelectedOpening._lastWall != null)
                {
                    WallMeshGenerator.GenerateWallWithOpenings(SelectedOpening._lastWall, createCol);
                }
                if (createCol && SelectedOpening._lastWall != null) { SelectedOpening._lastWall = null; }
            }
        }
    }

    private void ClearWallSegment()
    {
        if (SelectedOpening == null || SelectedOpening.ParentWall == null) return;
        foreach (GameObject go in SelectedOpening.ParentWall.WallSegmentColliders) { GameObject.Destroy(go); }
        SelectedOpening.ParentWall.WallSegmentColliders.Clear();
    }

    public void ShowResizer(T opening)
    {
        if (opening == null) return;
        SelectedOpening = opening;

        foreach (ResizeHandle handle in _resizeHandles)
        {
            if (handle != null) GameObject.Destroy(handle.gameObject);
        }
        _resizeHandles.Clear();

        if (SelectedOpening is Door) { ShowDoorResizer(); }
        else if (SelectedOpening is Window) { ShowWindowResizer(); }
    }

    private void ShowDoorResizer()
    {
        Debug.Log("Creating resize handles for a Door.");
        CreateHandles(CornerType.TopLeft);
        CreateHandles(CornerType.TopRight);
        CreateHandles(CornerType.midLeft);
        CreateHandles(CornerType.midRight);
        SetHandlesPosition();
    }

    // --- MODIFIED: Implemented for Window ---
    private void ShowWindowResizer()
    {
        Debug.Log("Creating resize handles for a Window.");
        CreateHandles(CornerType.TopLeft);
        CreateHandles(CornerType.TopRight);
        CreateHandles(CornerType.BottomLeft);
        CreateHandles(CornerType.BottomRight);
        SetHandlesPosition();
    }

    private void CreateHandles(CornerType cornerType)
    {
        if (_resizeHandlePrefab == null)
            _resizeHandlePrefab = Resources.Load<GameObject>("Prefab/ResizeHandlePrefab");
        ResizeHandle go = GameObject.Instantiate(_resizeHandlePrefab, SelectedOpening.transform).GetComponent<ResizeHandle>();
        go.ownerOpening = SelectedOpening;
        go.corner = cornerType;
        _resizeHandles.Add(go);
    }

    private void SetHandlesPosition()
    {
        if (SelectedOpening == null) return;
        if (SelectedOpening is Door) { SetDoorHandlesPosition(); }
        else if (SelectedOpening is Window) { SetWindowHandlesPosition(); }
    }

    private void SetDoorHandlesPosition()
    {
        float visibilityOffset = AppHelper._wallThickness / 2;
        foreach (ResizeHandle handle in _resizeHandles)
        {
            float y = 0, z = 0;
            switch (handle.corner)
            {
                case CornerType.TopRight: z = SelectedOpening.Width / 2f; y = SelectedOpening.Height; break;
                case CornerType.TopLeft: z = -SelectedOpening.Width / 2f; y = SelectedOpening.Height; break;
                case CornerType.midLeft: z = -SelectedOpening.Width / 2f; y = SelectedOpening.Height / 2; break;
                case CornerType.midRight: z = SelectedOpening.Width / 2f; y = SelectedOpening.Height / 2; break;
            }
            handle.transform.localPosition = new Vector3(visibilityOffset, y - handle.ownerOpening.OpeningPosition.y, z);
        }
    }

    // --- MODIFIED: Implemented for Window ---
    private void SetWindowHandlesPosition()
    {
        float visibilityOffset = AppHelper._wallThickness / 2;
        foreach (ResizeHandle handle in _resizeHandles)
        {
            float y = 0, z = 0;
            switch (handle.corner)
            {
                case CornerType.TopRight: z = SelectedOpening.Width / 2f; y = SelectedOpening.OpeningPosition.y - SelectedOpening.Height/2; break;
                case CornerType.TopLeft: z = -SelectedOpening.Width / 2f; y = SelectedOpening.OpeningPosition.y - SelectedOpening.Height / 2; break;
                case CornerType.BottomRight: z = SelectedOpening.Width / 2f; y = -(SelectedOpening.OpeningPosition.y - SelectedOpening.Height / 2); break;
                case CornerType.BottomLeft: z = -SelectedOpening.Width / 2f; y = -(SelectedOpening.OpeningPosition.y - SelectedOpening.Height / 2); break;
            }
            // This calculation works for Windows too because OpeningPosition.y correctly offsets
            // the local handle position relative to the opening's pivot.
            handle.transform.localPosition = new Vector3(visibilityOffset, y - handle.ownerOpening.OpeningPosition.y, z);
        }
    }

    private void ResizeOpening(Vector2 screenPos)
    {
        if (SelectedOpening.ParentWall == null || _currentSelectedHandle == null) return;
        Ray ray = _camera.ScreenPointToRay(screenPos);
        Plane wallPlane = new Plane(SelectedOpening.transform.right, SelectedOpening.transform.position);
        if (wallPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            if (SelectedOpening is Door) { ResizeDoor(hitPoint); }
            else if (SelectedOpening is Window) { ResizeWindow(hitPoint); }
        }
    }

    // --- MODIFIED: Refined for mid handles ---
    private void ResizeDoor(Vector3 hitPoint)
    {
        Transform openingTransform = SelectedOpening.transform;

        // Handle side handles (width only) separately
        if (_currentSelectedHandle.corner == CornerType.midLeft || _currentSelectedHandle.corner == CornerType.midRight)
        {
            Vector3 localHitPoint = openingTransform.InverseTransformPoint(hitPoint);
            float newWidth = Mathf.Abs(localHitPoint.z) * 2;

            // Anchor is the opposite side's center
            Vector3 anchorSideCenter = openingTransform.position;
            if (_currentSelectedHandle.corner == CornerType.midLeft)
            {
                anchorSideCenter += openingTransform.forward * (SelectedOpening.Width / 2f);
                SelectedOpening.transform.position = anchorSideCenter - openingTransform.forward * (newWidth / 2f);
            }
            else // midRight
            {
                anchorSideCenter -= openingTransform.forward * (SelectedOpening.Width / 2f);
                SelectedOpening.transform.position = anchorSideCenter + openingTransform.forward * (newWidth / 2f);
            }
            SelectedOpening.Width = newWidth;
        }
        else // Handle corner handles (width and height)
        {
            Vector3 geometricCenter = openingTransform.position + openingTransform.up * (SelectedOpening.Height / 2f);
            Vector3 anchorPoint = Vector3.zero;

            switch (_currentSelectedHandle.corner)
            {
                case CornerType.TopRight: anchorPoint = geometricCenter - openingTransform.forward * (SelectedOpening.Width / 2f) - openingTransform.up * (SelectedOpening.Height / 2f); break;
                case CornerType.TopLeft: anchorPoint = geometricCenter + openingTransform.forward * (SelectedOpening.Width / 2f) - openingTransform.up * (SelectedOpening.Height / 2f); break;
            }

            Vector3 newSizeVector = hitPoint - anchorPoint;
            float newWidth = Mathf.Abs(Vector3.Dot(newSizeVector, openingTransform.forward));
            float newHeight = Mathf.Abs(Vector3.Dot(newSizeVector, openingTransform.up));
            SelectedOpening.Width = newWidth;
            SelectedOpening.Height = newHeight;

            Vector3 newGeometricCenter = anchorPoint + (newSizeVector / 2f);
            SelectedOpening.transform.position = newGeometricCenter - openingTransform.up * (newHeight / 2f);
        }

        // Update everything after the resize logic
        SelectedOpening.OpeningPosition = SelectedOpening.ParentWall.transform.InverseTransformPoint(SelectedOpening.transform.position);
        SetHandlesPosition();
        WallMeshGenerator.GenerateWallWithOpenings(SelectedOpening.ParentWall, false);
        SetMeshRendererSize();
    }

    // --- MODIFIED: Implemented for Window ---
    private void ResizeWindow(Vector3 hitPoint)
    {
        Transform openingTransform = SelectedOpening.transform;
        Vector3 geometricCenter = openingTransform.position + openingTransform.up * (SelectedOpening.Height / 2f);
        Vector3 anchorPoint = Vector3.zero;

        // Determine the stationary anchor point based on which corner handle is being dragged
        switch (_currentSelectedHandle.corner)
        {
            case CornerType.TopRight:     // Anchor is BottomLeft
                anchorPoint = geometricCenter - openingTransform.forward * (SelectedOpening.Width / 2f) - openingTransform.up * (SelectedOpening.Height / 2f);
                break;
            case CornerType.TopLeft:      // Anchor is BottomRight
                anchorPoint = geometricCenter + openingTransform.forward * (SelectedOpening.Width / 2f) - openingTransform.up * (SelectedOpening.Height / 2f);
                break;
            case CornerType.BottomRight:  // Anchor is TopLeft
                anchorPoint = geometricCenter - openingTransform.forward * (SelectedOpening.Width / 2f) + openingTransform.up * (SelectedOpening.Height / 2f);
                break;
            case CornerType.BottomLeft:   // Anchor is TopRight
                anchorPoint = geometricCenter + openingTransform.forward * (SelectedOpening.Width / 2f) + openingTransform.up * (SelectedOpening.Height / 2f);
                break;
        }

        Vector3 newSizeVector = hitPoint - anchorPoint;

        float newWidth = Mathf.Abs(Vector3.Dot(newSizeVector, openingTransform.forward));
        float newHeight = Mathf.Abs(Vector3.Dot(newSizeVector, openingTransform.up));

        SelectedOpening.Width = newWidth;
        SelectedOpening.Height = newHeight;

        Vector3 newGeometricCenter = anchorPoint + (newSizeVector / 2f);
        SelectedOpening.transform.position = newGeometricCenter - openingTransform.up * (newHeight / 2f);

        SelectedOpening.OpeningPosition = SelectedOpening.ParentWall.transform.InverseTransformPoint(SelectedOpening.transform.position);
        SetHandlesPosition();
        WallMeshGenerator.GenerateWallWithOpenings(SelectedOpening.ParentWall, false);
        SetMeshRendererSize();
    }

    private void SetMeshRendererSize()
    {
        if (SelectedOpening._openingRedererGo == null) return;

        if (SelectedOpening is Door)
            SelectedOpening._openingRedererGo.transform.localScale = new Vector3(SelectedOpening.Width / AppHelper._doorWidth, 1f, SelectedOpening.Height / AppHelper._doorHeight);
        else if (SelectedOpening is Window)
            SelectedOpening._openingRedererGo.transform.localScale = new Vector3(SelectedOpening.Width / AppHelper._windowWidth, 1f, SelectedOpening.Height / AppHelper._windowHeight);

        SelectedOpening._openingRedererGo.transform.localPosition = Vector3.down * SelectedOpening.OpeningPosition.y;
    }

    #endregion
}