using Unity.VisualScripting;
using UnityEngine;

public class OrthoCam : CameraManager
{
    private Vector2 _initialScreenTouchPos;

    [SerializeField] private float _zoomSpeed = 1f;
    [SerializeField] private float _minOrthoSize = 2f;
    [SerializeField] private float _maxOrthoSize = 50f;

    [SerializeField] private float _moveThreshold = 10f;
    [SerializeField] private float _lerpSpeed = 3f;

    private Vector3 _targetPosition;
    [SerializeField] private float _targetOrthoSize;

    // Bounds for camera movement
    [SerializeField] private Vector2 _minBounds = new Vector2(-50, -50);
    [SerializeField] private Vector2 _maxBounds = new Vector2(50, 50);

    #region Properties

    public float TranslationSpeed
    {
        get
        {
            float t = Mathf.InverseLerp(_minOrthoSize, _maxOrthoSize, _targetOrthoSize);

            return Mathf.Lerp(10, 25, t);
        }
        set
        {
            _translationSpeed = value;
        }
    }

    #endregion

    #region Getter and Setter

    public float GetMinOrthoSize()
    {
        return _minOrthoSize;
    }

    public float GetMaxOrthoSize()
    {
        return _maxOrthoSize;
    }

    #endregion


    #region Get/Set Touch Position
    public Vector2 GetInitialScreenTouchPosition() => _initialScreenTouchPos;
    public void SetInitialTouchPosition(Vector2 position) => _initialScreenTouchPos = position;
    #endregion

    public void Start()
    {
        if (_mainCamera != null && _mainCamera.orthographic)
            _targetOrthoSize = _mainCamera.orthographicSize;
    }

    public void Update()
    {
        if (_mainCamera == null || !_mainCamera.orthographic) return;

        _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize,_targetOrthoSize,Time.deltaTime * 5f);

        ClampPosition();
    }

    public void MoveCameraByDistance(Vector3 distance, Vector2 currentTouchPos)
    {
        float screenDist = Vector2.Distance(currentTouchPos, _initialScreenTouchPos);
        if (screenDist < _moveThreshold) return;

        // Scale translation speed inversely with zoom
        float zoomFactor = _mainCamera.orthographicSize / _targetOrthoSize;
        float adjustedSpeed = TranslationSpeed * (_minOrthoSize / _mainCamera.orthographicSize);

        // Convert screen drag to world delta
        Vector3 worldDelta = new Vector3(distance.x, 0, distance.z) * (adjustedSpeed * Time.deltaTime);

        // Smooth damp movement
        _targetPosition = _mainCamera.transform.position + worldDelta;
        _mainCamera.transform.position = Vector3.SmoothDamp(
            _mainCamera.transform.position,
            _targetPosition,
            ref _velocity,
            0.15f
        );

        ClampPosition();
    }


    // velocity ref for SmoothDamp
    private Vector3 _velocity = Vector3.zero;


    public Vector3 GetDistance(Vector2 finalPosition)
    {
        Vector2 distance = _initialScreenTouchPos - finalPosition;
        return new Vector3(distance.x, 0, distance.y);
    }

    public void ZoomCamera(float deltaMagnitudeDiff)
    {
        if (_mainCamera == null || !_mainCamera.orthographic) return;

        _targetOrthoSize -= deltaMagnitudeDiff * _zoomSpeed * Time.deltaTime;
        _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);
    }

    private void ClampPosition()
    {
        Vector3 pos = _mainCamera.transform.position;
        pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x);
        pos.z = Mathf.Clamp(pos.z, _minBounds.y, _maxBounds.y);
        _mainCamera.transform.position = pos;
    }

    public void FitToAllWalls()
    {
        if (WallManager.Instance == null) return;

        Bounds wallBounds = WallManager.Instance.GetSceneBounds();
        if (wallBounds.size == Vector3.zero) return;

        FitToDrawing(wallBounds);
    }

    public void FitToDrawing(Bounds drawingBounds)
    {
        // Center camera on drawing
        Vector3 center = drawingBounds.center;
        _targetPosition = new Vector3(center.x, _mainCamera.transform.position.y, center.z);

        // Adjust ortho size to fit bounds
        float sizeX = drawingBounds.size.x / _mainCamera.aspect / 2f;
        float sizeZ = drawingBounds.size.z / 2f;
        float targetSize = Mathf.Max(sizeX, sizeZ);

        _targetOrthoSize = Mathf.Clamp(targetSize, _minOrthoSize, _maxOrthoSize);
    }

}
