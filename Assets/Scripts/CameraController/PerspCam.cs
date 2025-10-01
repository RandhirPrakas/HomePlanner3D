using UnityEngine;

public class PerspCam : CameraManager
{
    [Header("Orbit Settings")]
    [SerializeField] private Vector3 _target = Vector3.zero;
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _zoomSpeed = 0.1f;
    [SerializeField] private float _rotationSpeed = 0.2f;
    [SerializeField] private float _panSpeed = 0.005f;
    [SerializeField] private float _lerpSpeed = 8f;

    [Header("Pitch Clamp")]
    [SerializeField] private float _minPitch = 10f;
    [SerializeField] private float _maxPitch = 80f;

    private float _yaw = 0f;
    private float _pitch = 45f;
    private float _targetYaw;
    private float _targetPitch;
    private float _targetDistance;

    private Vector2 _lastTouchPos;

    private Camera _cam;

    #region Getter and Setter

    public float GetCurrentDistance()
    {
        return _distance;
    }

    #endregion

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        _targetYaw = _yaw;
        _targetPitch = _pitch;
        _targetDistance = _distance;
    }

    public void SetInitialTouchPosition(Vector2 screenPos)
    {
        _lastTouchPos = screenPos;
    }

    public void RotateCamera(Vector2 screenPos)
    {
        Vector2 delta = screenPos - _lastTouchPos;

        _targetYaw += delta.x * _rotationSpeed;
        _targetPitch -= delta.y * _rotationSpeed;

        _lastTouchPos = screenPos;
    }

    public void PanCamera(Vector2 screenPos)
    {
        Vector2 delta = screenPos - _lastTouchPos;

        // Right and up directions in world space based on current camera orientation
        Vector3 right = _cam.transform.right;
        Vector3 up = _cam.transform.up;

        // Apply delta
        Vector3 panMovement = (-right * delta.x + -up * delta.y) * _panSpeed * _distance;
        _target += panMovement;

        _lastTouchPos = screenPos;
    }

    public void ZoomCamera(float delta)
    {
        _targetDistance -= delta * _zoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, 15f, 50f);
    }

    public void UpdateCamera()
    {
        if (_cam == null || _cam.orthographic) return;

        _yaw = Mathf.Lerp(_yaw, _targetYaw, Time.deltaTime * _lerpSpeed);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, Time.deltaTime * _lerpSpeed);
        _distance = Mathf.Lerp(_distance, _targetDistance, Time.deltaTime * _lerpSpeed);

        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);



        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -_distance);
        _cam.transform.position = _target + offset;
        _cam.transform.LookAt(_target);
    }

    public void PanCameraByDelta(Vector2 delta)
    {
        Vector3 forward = _cam.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = _cam.transform.right;

        Vector3 panMovement = (-right * delta.x + -forward * delta.y) * _panSpeed * _distance * Time.deltaTime;
        _target += panMovement;
    }

}
