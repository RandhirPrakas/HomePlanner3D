using UnityEngine;

public class OrthoCam : CameraManager
{
    private Vector2 _initialScreenTouchPos;

    [SerializeField] private float _zoomSpeed = 1f; // default 1
    [SerializeField] private float _minOrthoSize = 2f;
    [SerializeField] private float _maxOrthoSize = 50f;

    [SerializeField] private float _moveThreshold = 10f;
    [SerializeField] private float _lerpSpeed = 3f; // default = 3, at this value looks smooth

    private Vector3 _targetPosition;
    
    [SerializeField] private float _targetOrthoSize;


    #region Get/Set Touch Position
    public Vector2 GetInitialScreenTouchPosition()
    {
        return _initialScreenTouchPos;
    }

    public void SetInitialTouchPosition(Vector2 position)
    {
        _initialScreenTouchPos = position;
    }
    #endregion

    public void Update()
    {
        if (_mainCamera == null || !_mainCamera.orthographic) return;

        _mainCamera.orthographicSize = Mathf.Lerp(
            _mainCamera.orthographicSize,
            _targetOrthoSize,
            Time.deltaTime * 5f
        );
    }

    /*public void MoveCameraByDistance(Vector3 distance)
    {
        _mainCamera.transform.position += distance * _translationSpeed * Time.deltaTime;
    }*/

    public void MoveCameraByDistance(Vector3 distance, Vector2 currentTouchPos)
    {
        // Check if drag exceeds threshold (prevents jitter)
        float screenDist = Vector2.Distance(currentTouchPos, _initialScreenTouchPos);
        if (screenDist < _moveThreshold) return;

        // Set new target position
        _targetPosition = _mainCamera.transform.position + distance * _translationSpeed;

        // Smoothly move with Lerp
        _mainCamera.transform.position = Vector3.Lerp(
            _mainCamera.transform.position,
            _targetPosition,
            _lerpSpeed * Time.deltaTime
        );
    }

    public Vector3 GetDistance(Vector2 finalPosition)
    {
        Vector2 distance = (_initialScreenTouchPos - finalPosition).normalized;
        return new Vector3(distance.x, 0, distance.y);
    }

    public void ZoomCamera(float deltaMagnitudeDiff)
    {
        if (_mainCamera == null || !_mainCamera.orthographic) return;

        _targetOrthoSize -= deltaMagnitudeDiff * _zoomSpeed * Time.deltaTime;
        _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);
    }
}
