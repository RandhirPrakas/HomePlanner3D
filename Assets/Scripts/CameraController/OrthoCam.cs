using UnityEngine;

public class OrthoCam : CameraManager
{
    private Vector2 _initialScreenTouchPos;

    [SerializeField] private float _zoomSpeed = 1f; // default 1
    [SerializeField] private float _minOrthoSize = 2f;
    [SerializeField] private float _maxOrthoSize = 50f;

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

    public void MoveCameraByDistance(Vector3 distance)
    {
        _mainCamera.transform.position += distance * _translationSpeed * Time.deltaTime;
    }

    public Vector3 GetDistance(Vector2 finalPosition)
    {
        Vector2 distance = (_initialScreenTouchPos - finalPosition).normalized;
        return new Vector3(distance.x, 0, distance.y);
    }

    public void ZoomCamera(float deltaMagnitudeDiff)
    {
        if (_mainCamera == null || !_mainCamera.orthographic) return;

        _mainCamera.orthographicSize -= deltaMagnitudeDiff * _zoomSpeed * Time.deltaTime;
        _mainCamera.orthographicSize = Mathf.Clamp(_mainCamera.orthographicSize, _minOrthoSize, _maxOrthoSize);
    }
}
